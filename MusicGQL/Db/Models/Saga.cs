using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using MusicGQL.Sagas.DownloadRelease;

namespace MusicGQL.Db.Models;

[Table("sagas")]
public class Saga
{
    [Key] [Column("id")] public Guid Id { get; set; }

    [Column("revision")] public int Revision { get; set; }

    [Column("data")] public byte[] Data { get; set; } = default!;
}