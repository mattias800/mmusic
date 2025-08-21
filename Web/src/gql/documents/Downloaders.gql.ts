import { graphql } from "@/gql";

export const DownloadersTogglesCardDocument = graphql(`
  query DownloadersTogglesCard {
    serverSettings {
      id
      enableSabnzbdDownloader
      enableQBittorrentDownloader
      enableSoulSeekDownloader
    }
  }
`);

export const UpdateDownloaderSettingsDocument = graphql(`
  mutation UpdateDownloaderSettings($input: UpdateDownloaderSettingsInput!) {
    updateDownloaderSettings(input: $input) {
      __typename
      ... on UpdateDownloaderSettingsSuccess {
        serverSettings {
          id
          enableSabnzbdDownloader
          enableQBittorrentDownloader
          enableSoulSeekDownloader
        }
      }
      ... on UpdateDownloaderSettingsError {
        message
      }
    }
  }
`);


