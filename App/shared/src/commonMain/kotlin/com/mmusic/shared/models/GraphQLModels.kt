package com.mmusic.shared.models

import kotlinx.serialization.Serializable

@Serializable
data class GraphQLRequest(
    val query: String,
    val variables: Map<String, String>? = null
)

@Serializable
data class GraphQLResponse<T>(
    val data: T? = null,
    val errors: List<GraphQLError>? = null
)

@Serializable
data class GraphQLError(
    val message: String
)

// Bootstrap Query Models
@Serializable
data class BootstrapData(
    val areThereAnyUsers: Boolean,
    val viewer: ViewerData? = null
)

@Serializable
data class ViewerData(
    val id: String,
    val username: String
)

// SignIn Mutation Models
@Serializable
data class SignInData(
    val signIn: SignInResult
)

@Serializable
data class SignInResult(
    val __typename: String,
    val user: UserData? = null,
    val token: String? = null,
    val message: String? = null
)

@Serializable
data class UserData(
    val id: String,
    val username: String
)
