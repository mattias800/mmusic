CREATE TABLE users
(
    id         uuid PRIMARY KEY,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    username   VARCHAR(255) NOT NULL
);

INSERT INTO users (username)
VALUES ('test_user_1');

CREATE TABLE events
(
    id         BIGSERIAL PRIMARY KEY NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    event_type VARCHAR(255)          NOT NULL,
    event_data JSONB                 NOT NULL
);

CREATE TABLE event_checkpoint
(
    id                      SERIAL PRIMARY KEY,
    last_processed_event_id BIGSERIAL NOT NULL,
    FOREIGN KEY (last_processed_event_id) REFERENCES events (id)
);

CREATE TABLE liked_songs_projection
(
    id           SERIAL PRIMARY KEY,
    user_id      SERIAL NOT NULL,
    recording_id uuid   NOT NULL,
    FOREIGN KEY (user_id) REFERENCES users (id),
    FOREIGN KEY (recording_id) REFERENCES recordings (id)
);

CREATE TABLE artists_added_to_server_library_projection
(
    id        SERIAL PRIMARY KEY,
    added_by  SERIAL NOT NULL,
    artist_id uuid   NOT NULL,
    FOREIGN KEY (added_by) REFERENCES users (id),
    FOREIGN KEY (artist_id) REFERENCES artists (id)
);

CREATE TABLE release_groups_added_to_server_library_projection
(
    id               SERIAL PRIMARY KEY,
    added_by         SERIAL NOT NULL,
    release_group_id uuid   NOT NULL,
    FOREIGN KEY (added_by) REFERENCES users (id),
    FOREIGN KEY (release_group_id) REFERENCES release_groups (id)
);

CREATE TABLE artists
(
    id        uuid PRIMARY KEY,
    name      VARCHAR(255) NOT NULL,
    sort_name VARCHAR(255) NOT NULL,
    gender    VARCHAR(255)
);

CREATE TABLE release_groups
(
    id                 uuid PRIMARY KEY,
    title              VARCHAR(255) NOT NULL,
    primary_type       VARCHAR(255),
    secondary_types    VARCHAR(255),
    first_release_date VARCHAR(255)
);

CREATE TABLE releases
(
    id      uuid PRIMARY KEY,
    title   VARCHAR(255) NOT NULL,
    date    VARCHAR(255),
    barcode VARCHAR(255),
    country VARCHAR(255),
    status  VARCHAR(255),
    quality VARCHAR(255)
);

