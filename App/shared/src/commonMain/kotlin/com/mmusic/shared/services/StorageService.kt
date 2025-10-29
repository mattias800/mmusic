package com.mmusic.shared.services

import com.russhwolf.settings.Settings
import com.russhwolf.settings.get
import com.russhwolf.settings.set

class StorageService(private val settings: Settings = Settings()) {

    companion object {
        private const val SERVER_URL_KEY = "server_url"
        private const val AUTH_TOKEN_KEY = "auth_token"
    }

    fun setServerUrl(url: String) {
        settings[SERVER_URL_KEY] = url
    }

    fun getServerUrl(): String? {
        return settings.getStringOrNull(SERVER_URL_KEY)
    }

    fun hasServerUrl(): Boolean {
        return getServerUrl() != null
    }

    fun clearServerUrl() {
        settings.remove(SERVER_URL_KEY)
    }

    fun setAuthToken(token: String) {
        settings[AUTH_TOKEN_KEY] = token
    }

    fun getAuthToken(): String? {
        return settings.getStringOrNull(AUTH_TOKEN_KEY)
    }

    fun hasAuthToken(): Boolean {
        return getAuthToken() != null
    }

    fun clearAuthToken() {
        settings.remove(AUTH_TOKEN_KEY)
    }

    fun clearAll() {
        clearServerUrl()
        clearAuthToken()
    }
}
