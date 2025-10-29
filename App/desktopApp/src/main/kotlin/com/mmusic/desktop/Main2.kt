package com.mmusic.desktop

import androidx.compose.desktop.ui.tooling.preview.Preview
import androidx.compose.runtime.*
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Window
import androidx.compose.ui.window.application
import androidx.compose.ui.window.rememberWindowState
import com.mmusic.shared.services.ApolloGraphQLClient
import com.mmusic.shared.services.StorageService

fun main() = application {
    val windowState = rememberWindowState(width = 1400.dp, height = 900.dp)

    Window(
        onCloseRequest = ::exitApplication,
        title = "MMusic - Desktop",
        state = windowState
    ) {
        AppNew()
    }
}

@Composable
@Preview
fun AppNew() {
    val storageService = remember { StorageService() }
    val apolloClient = remember { ApolloGraphQLClient(storageService) }

    MainApp(
        storageService = storageService,
        apolloClient = apolloClient
    )
}
