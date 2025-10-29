package com.mmusic.desktop.navigation

sealed class Route(val path: String) {
    object Home : Route("home")
    object Artists : Route("artists")
    object Albums : Route("albums")
    data class ArtistDetail(val artistId: String) : Route("artist/$artistId")
    data class AlbumDetail(val artistId: String, val folderName: String) : Route("artist/$artistId/album/$folderName")
    object Search : Route("search")
    object Settings : Route("settings")
}
