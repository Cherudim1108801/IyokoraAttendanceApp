namespace IyokoraAttendanceApp.Models;

/// <summary>1回分の練習予定（Firestore の <c>practices</c> ドキュメントに対応）。</summary>
public class Practice
{
    /// <summary>練習予定ID（Firestore のドキュメントID）。</summary>
    public required string Id { get; set; }

    /// <summary>練習日。カレンダー日付として扱い、時刻情報は持たない。</summary>
    public DateTime Date { get; set; }

    /// <summary>タイトル（任意）。</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>場所（任意）。</summary>
    public string Place { get; set; } = string.Empty;

    /// <summary>登録日時（UTC）。</summary>
    public DateTime CreatedAt { get; set; }
}
