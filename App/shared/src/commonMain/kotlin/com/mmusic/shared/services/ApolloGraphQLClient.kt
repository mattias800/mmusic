package com.mmusic.shared.services

import com.apollographql.apollo.ApolloClient
import com.apollographql.apollo.api.ApolloResponse
import com.apollographql.apollo.api.Query
import com.apollographql.apollo.api.Mutation
import com.mmusic.shared.graphql.*
import com.mmusic.shared.graphql.type.*

class ApolloGraphQLClient(private val storageService: StorageService) {
    private var apolloClient: ApolloClient? = null

    init {
        recreateClient()
    }

    fun recreateClient() {
        val serverUrl = storageService.getServerUrl() ?: return
        val token = storageService.getAuthToken()

        apolloClient = ApolloClient.Builder()
            .serverUrl("$serverUrl/graphql")
            .apply {
                if (!token.isNullOrBlank()) {
                    addHttpHeader("Authorization", "Bearer $token")
                }
            }
            .build()
    }

    // Generic query method
    suspend fun <D : Query.Data> query(query: Query<D>): ApolloResponse<D> {
        val client = apolloClient ?: throw IllegalStateException("Apollo client not initialized")
        return client.query(query).execute()
    }

    // Generic mutation method
    suspend fun <D : Mutation.Data> mutate(mutation: Mutation<D>): ApolloResponse<D> {
        val client = apolloClient ?: throw IllegalStateException("Apollo client not initialized")
        return client.mutation(mutation).execute()
    }

    // Convenience methods for common queries
    suspend fun bootstrap(): ApolloResponse<BootstrapQuery.Data> {
        return query(BootstrapQuery())
    }

    suspend fun getArtists(): ApolloResponse<GetArtistsQuery.Data> {
        return query(GetArtistsQuery())
    }

    suspend fun getAlbums(): ApolloResponse<GetAlbumsQuery.Data> {
        return query(GetAlbumsQuery())
    }

    suspend fun getArtistDetail(artistId: String): ApolloResponse<GetArtistDetailQuery.Data> {
        return query(GetArtistDetailQuery(artistId))
    }

    suspend fun getAlbumDetail(artistId: String, releaseFolderName: String): ApolloResponse<GetAlbumDetailQuery.Data> {
        return query(GetAlbumDetailQuery(artistId, releaseFolderName))
    }

    suspend fun search(searchTerm: String, limit: Int = 20): ApolloResponse<SearchQuery.Data> {
        return query(SearchQuery(searchTerm, com.apollographql.apollo.api.Optional.present(limit)))
    }

    // Playlist queries
    suspend fun getPlaylists(): ApolloResponse<GetPlaylistsQuery.Data> {
        return query(GetPlaylistsQuery())
    }

    suspend fun getPlaylistsWithDetails(): ApolloResponse<GetPlaylistsWithDetailsQuery.Data> {
        return query(GetPlaylistsWithDetailsQuery())
    }

    // Playlist mutations
    suspend fun createPlaylist(name: String): ApolloResponse<CreatePlaylistMutation.Data> {
        return mutate(CreatePlaylistMutation(CreatePlaylistInput(name = com.apollographql.apollo.api.Optional.present(name))))
    }

    suspend fun deletePlaylist(playlistId: String): ApolloResponse<DeletePlaylistMutation.Data> {
        return mutate(DeletePlaylistMutation(DeletePlaylistInput(playlistId = playlistId)))
    }

    suspend fun renamePlaylist(playlistId: String, name: String): ApolloResponse<RenamePlaylistMutation.Data> {
        return mutate(RenamePlaylistMutation(RenamePlaylistInput(newPlaylistName = name, playlistId = playlistId)))
    }

    suspend fun addTrackToPlaylist(
        playlistId: String,
        artistId: String,
        releaseFolderName: String,
        trackNumber: Int
    ): ApolloResponse<AddTrackToPlaylistMutation.Data> {
        return mutate(
            AddTrackToPlaylistMutation(
                AddTrackToPlaylistInput(
                    playlistId = playlistId,
                    artistId = artistId,
                    releaseFolderName = releaseFolderName,
                    trackNumber = trackNumber
                )
            )
        )
    }

    suspend fun removeItemFromPlaylist(playlistId: String, itemId: String): ApolloResponse<RemoveItemFromPlaylistMutation.Data> {
        return mutate(RemoveItemFromPlaylistMutation(RemoveItemFromPlaylistInput(playlistId = playlistId, playlistItemId = itemId)))
    }

    suspend fun movePlaylistItem(playlistId: String, itemId: String, newIndex: Int): ApolloResponse<MovePlaylistItemMutation.Data> {
        return mutate(MovePlaylistItemMutation(MovePlaylistItemInput(playlistId = playlistId, playlistItemId = itemId, newIndex = newIndex)))
    }
}
