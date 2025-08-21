import { graphql } from "@/gql";

export const updateDownloaderSettingsMutation = graphql(`
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

export const downloadersTogglesCardQuery = graphql(`
  query DownloadersTogglesCard {
    serverSettings {
      id
      enableSabnzbdDownloader
      enableQBittorrentDownloader
      enableSoulSeekDownloader
    }
  }
`);
