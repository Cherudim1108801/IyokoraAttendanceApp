using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

/// <summary>
/// 練習の演奏予定曲(<c>pieces</c> フィールド内の1曲分のマップ)と録音一覧の相互変換を担う純粋ロジック。
/// 新スキーマ(<c>recordings</c> 配列)を優先しつつ、旧スキーマ(<c>recordingUrl</c>/<c>featured</c> の単一録音)の
/// ドキュメントも読み込めるよう後方互換の変換を行う。次回の書き込みで新スキーマへ自動的に移行される。
/// </summary>
public static class PracticeRecordingMapper
{
    /// <summary>1曲分のフィールドから録音一覧を読み込む。</summary>
    public static List<PracticeRecording> ToRecordings(Dictionary<string, object?> pieceFields)
    {
        if (pieceFields.TryGetValue("recordings", out var raw) && raw is List<object?> list)
        {
            return list
                .OfType<Dictionary<string, object?>>()
                .Select(r => new PracticeRecording
                {
                    Id = r.GetValueOrDefault("id") as string ?? Guid.NewGuid().ToString("N"),
                    Name = r.GetValueOrDefault("name") as string ?? string.Empty,
                    Url = r.GetValueOrDefault("url") as string ?? string.Empty,
                    IsFeatured = r.GetValueOrDefault("featured") as bool? ?? false
                })
                .ToList();
        }

        // 旧スキーマ（1曲1件の recordingUrl/featured）からのフォールバック変換。
        var legacyUrl = pieceFields.GetValueOrDefault("recordingUrl") as string;
        if (string.IsNullOrEmpty(legacyUrl))
            return [];

        return
        [
            new PracticeRecording
            {
                Id = Guid.NewGuid().ToString("N"),
                Url = legacyUrl,
                IsFeatured = pieceFields.GetValueOrDefault("featured") as bool? ?? false
            }
        ];
    }

    /// <summary>録音一覧を、Firestore に保存する <c>recordings</c> フィールドの値に変換する。</summary>
    public static List<object?> ToRecordingFields(IReadOnlyList<PracticeRecording> recordings) => recordings
        .Select(r => new Dictionary<string, object?>
        {
            ["id"] = r.Id,
            ["name"] = r.Name,
            ["url"] = r.Url,
            ["featured"] = r.IsFeatured
        })
        .Cast<object?>()
        .ToList();
}
