package com.mmusic.shared.services

import com.mmusic.shared.models.*
import io.ktor.client.*
import io.ktor.client.call.*
import io.ktor.client.plugins.*
import io.ktor.client.plugins.auth.*
import io.ktor.client.plugins.auth.providers.*
import io.ktor.client.plugins.contentnegotiation.*
import io.ktor.client.plugins.logging.*
import io.ktor.client.request.*
import io.ktor.http.*
import io.ktor.serialization.kotlinx.json.*
import kotlinx.serialization.json.Json

class GraphQLClient(private val storageService: StorageService) {

    private var httpClient: HttpClient? = null

    private fun getOrCreateClient(): HttpClient {
        if (httpClient == null) {
            val token = storageService.getAuthToken()

            httpClient = HttpClient {
                install(ContentNegotiation) {
                    json(Json {
                        ignoreUnknownKeys = true
                        isLenient = true
                        prettyPrint = true
                    })
                }

                install(Logging) {
                    logger = Logger.DEFAULT
                    level = LogLevel.INFO
                }

                install(HttpTimeout) {
                    requestTimeoutMillis = 30000
                }

                // Add JWT auth if token exists
                if (!token.isNullOrBlank()) {
                    install(Auth) {
                        bearer {
                            loadTokens {
                                BearerTokens(token, token)
                            }
                        }
                    }
                }

                defaultRequest {
                    val serverUrl = storageService.getServerUrl()
                    if (!serverUrl.isNullOrBlank()) {
                        url("$serverUrl/graphql")
                    }
                    contentType(ContentType.Application.Json)
                }
            }
        }
        return httpClient!!
    }

    fun recreateClient() {
        httpClient?.close()
        httpClient = null
    }

    suspend fun bootstrap(): GraphQLResponse<BootstrapData> {
        val query = """
            query Bootstrap {
                areThereAnyUsers
                viewer {
                    id
                    username
                }
            }
        """.trimIndent()

        return executeQuery(GraphQLRequest(query))
    }

    suspend fun signIn(username: String, password: String): GraphQLResponse<SignInData> {
        val query = """
            mutation SignIn(${'$'}username: String!, ${'$'}password: String!) {
                signIn(input: { username: ${'$'}username, password: ${'$'}password }) {
                    __typename
                    ... on SignInSuccess {
                        user {
                            id
                            username
                        }
                        token
                    }
                    ... on SignInError {
                        message
                    }
                }
            }
        """.trimIndent()

        val variables = mapOf(
            "username" to username,
            "password" to password
        )

        return executeQuery(GraphQLRequest(query, variables))
    }

    private suspend inline fun <reified T> executeQuery(request: GraphQLRequest): GraphQLResponse<T> {
        val client = getOrCreateClient()

        return try {
            val response: GraphQLResponse<T> = client.post {
                setBody(request)
            }.body()

            response
        } catch (e: Exception) {
            GraphQLResponse(
                data = null,
                errors = listOf(GraphQLError("Network error: ${e.message}"))
            )
        }
    }
}
