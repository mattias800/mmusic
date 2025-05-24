using MusicGQL.Features.ServerLibrary.Recording.Db;

namespace MusicGQL.Integration.Neo4j.Models;

public record RecordingWithTrackInfo(DbRecording DbRecording, int TrackPosition);
