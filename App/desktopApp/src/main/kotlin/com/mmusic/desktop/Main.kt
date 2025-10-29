package com.mmusic.desktop

import androidx.compose.desktop.ui.tooling.preview.Preview
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.compose.ui.window.rememberWindowState
import com.mmusic.shared.services.GraphQLClient
import com.mmusic.shared.services.StorageService
import kotlinx.coroutines.launch

fun main() = application {
    val windowState = rememberWindowState(width = 1000.dp, height = 700.dp)

    Window(
        onCloseRequest = ::exitApplication,
        title = "MMusic",
        state = windowState
    ) {
        App()
    }
}

@Composable
@Preview
fun App() {
    val storageService = remember { StorageService() }
    val graphQLClient = remember { GraphQLClient(storageService) }

    var currentScreen by remember { mutableStateOf<Screen>(Screen.Loading) }
    val coroutineScope = rememberCoroutineScope()

    // Check authentication on startup
    LaunchedEffect(Unit) {
        if (!storageService.hasServerUrl()) {
            currentScreen = Screen.ServerSetup
        } else {
            val result = graphQLClient.bootstrap()
            val data = result.data  // Store in local variable for smart cast
            currentScreen = when {
                result.errors != null -> Screen.ServerSetup
                data?.viewer != null -> Screen.Home(data.viewer!!.username)
                else -> Screen.Login
            }
        }
    }

    MaterialTheme(
        colorScheme = darkColorScheme()
    ) {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            when (val screen = currentScreen) {
                is Screen.Loading -> LoadingScreen()
                is Screen.ServerSetup -> ServerSetupScreen(
                    onServerConfigured = {
                        currentScreen = Screen.Login
                    },
                    storageService = storageService,
                    graphQLClient = graphQLClient,
                    coroutineScope = coroutineScope
                )
                is Screen.Login -> LoginScreen(
                    onLoginSuccess = { username ->
                        currentScreen = Screen.Home(username)
                    },
                    onChangeServer = {
                        storageService.clearAll()
                        currentScreen = Screen.ServerSetup
                    },
                    storageService = storageService,
                    graphQLClient = graphQLClient,
                    coroutineScope = coroutineScope
                )
                is Screen.Home -> HomeScreen(
                    username = screen.username,
                    serverUrl = storageService.getServerUrl() ?: "Unknown",
                    onSignOut = {
                        storageService.clearAuthToken()
                        currentScreen = Screen.Login
                    },
                    onChangeServer = {
                        storageService.clearAll()
                        currentScreen = Screen.ServerSetup
                    }
                )
            }
        }
    }
}

sealed class Screen {
    object Loading : Screen()
    object ServerSetup : Screen()
    object Login : Screen()
    data class Home(val username: String) : Screen()
}
