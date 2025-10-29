package com.mmusic.desktop

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.mmusic.shared.services.GraphQLClient
import com.mmusic.shared.services.StorageService
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.launch

@Composable
fun ServerSetupScreen(
    onServerConfigured: () -> Unit,
    storageService: StorageService,
    graphQLClient: GraphQLClient,
    coroutineScope: CoroutineScope
) {
    var serverUrl by remember { mutableStateOf("") }
    var errorMessage by remember { mutableStateOf<String?>(null) }
    var isLoading by remember { mutableStateOf(false) }

    Box(
        modifier = Modifier.fillMaxSize().padding(32.dp),
        contentAlignment = Alignment.Center
    ) {
        Card(
            modifier = Modifier.width(500.dp),
            elevation = CardDefaults.cardElevation(defaultElevation = 4.dp)
        ) {
            Column(
                modifier = Modifier.padding(32.dp),
                verticalArrangement = Arrangement.spacedBy(16.dp)
            ) {
                Text(
                    text = "🎵 MMusic",
                    fontSize = 42.sp,
                    fontWeight = FontWeight.Bold,
                    color = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.align(Alignment.CenterHorizontally)
                )

                Spacer(modifier = Modifier.height(8.dp))

                Text(
                    text = "Welcome!",
                    fontSize = 28.sp,
                    fontWeight = FontWeight.Bold,
                    modifier = Modifier.align(Alignment.CenterHorizontally)
                )

                Text(
                    text = "To get started, please enter your MMusic server URL",
                    style = MaterialTheme.typography.bodyMedium,
                    modifier = Modifier.align(Alignment.CenterHorizontally)
                )

                Spacer(modifier = Modifier.height(16.dp))

                OutlinedTextField(
                    value = serverUrl,
                    onValueChange = { serverUrl = it; errorMessage = null },
                    label = { Text("Server URL") },
                    placeholder = { Text("https://your-server.com") },
                    modifier = Modifier.fillMaxWidth(),
                    enabled = !isLoading,
                    isError = errorMessage != null,
                    supportingText = if (errorMessage != null) {
                        { Text(errorMessage!!, color = MaterialTheme.colorScheme.error) }
                    } else null
                )

                Button(
                    onClick = {
                        if (serverUrl.isBlank()) {
                            errorMessage = "Please enter a server URL"
                            return@Button
                        }

                        isLoading = true
                        errorMessage = null

                        coroutineScope.launch {
                            try {
                                val normalizedUrl = if (!serverUrl.startsWith("http://") && !serverUrl.startsWith("https://")) {
                                    "https://$serverUrl"
                                } else {
                                    serverUrl
                                }.trimEnd('/')

                                storageService.setServerUrl(normalizedUrl)
                                graphQLClient.recreateClient()

                                // Test connection
                                val result = graphQLClient.bootstrap()
                                val errors = result.errors  // Store in local variable for smart cast

                                if (errors != null) {
                                    errorMessage = errors.firstOrNull()?.message ?: "Failed to connect to server"
                                    storageService.clearServerUrl()
                                } else {
                                    onServerConfigured()
                                }
                            } catch (e: Exception) {
                                errorMessage = "Failed to connect: ${e.message}"
                                storageService.clearServerUrl()
                            } finally {
                                isLoading = false
                            }
                        }
                    },
                    modifier = Modifier.fillMaxWidth().height(50.dp),
                    enabled = !isLoading
                ) {
                    if (isLoading) {
                        CircularProgressIndicator(
                            modifier = Modifier.size(24.dp),
                            color = MaterialTheme.colorScheme.onPrimary
                        )
                    } else {
                        Text("Continue")
                    }
                }

                Text(
                    text = "Examples: http://localhost:5095 or https://mmusic.example.com",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.align(Alignment.CenterHorizontally)
                )
            }
        }
    }
}
