using IyokoraAttendanceApp.Models;

namespace IyokoraAttendanceApp.Services;

/// <summary>
/// 練習の演奏予定曲一覧を、選択し直した曲の一覧で更新するための純粋ロジック。
/// 選択後も残っている曲は録音情報を維持し、新規に選択された曲は空の録音一覧で追加し、
/// 選択解除された曲は結果から除外する。
/// </summary>
public static class PracticePieceMerger
{
    /// <summary>既存の演奏予定曲一覧と、選択し直した曲(ID・曲名)の一覧を統合する。</summary>
    /// <param name="existingPieces">録音情報を保持している、現在の演奏予定曲一覧。</param>
    /// <param name="selectedPieces">選択後の曲の一覧(ID・曲名)。</param>
    public static List<PracticePieceRef> MergeSelectedPieces(
        IReadOnlyList<PracticePieceRef> existingPieces,
        IReadOnlyList<(string PieceId, string Title)> selectedPieces)
    {
        var existingByPieceId = existingPieces.ToDictionary(p => p.PieceId);

        return selectedPieces
            .Select(s => existingByPieceId.TryGetValue(s.PieceId, out var existing)
                ? existing
                : new PracticePieceRef { PieceId = s.PieceId, Title = s.Title })
            .ToList();
    }
}
