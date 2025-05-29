import { graphql } from "@/gql";
import * as React from "react";

export interface PlaylistListProps {

}

export const playlistListQuery = graphql(`
query PlaylistList  {
    viewer {
      
    }
}
`);

export const PlaylistList: React.FC<PlaylistListProps> = () => {
    return (
        <div></div>
    )
}
