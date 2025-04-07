namespace hoge.Configuration;

/// <summary>
/// アプリケーションの設定
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Notionの認証トークン
    /// </summary>
    /// <value></value>
    public string NotionAuthToken { get; private set; } = string.Empty;

    /// <summary>
    /// NotionのデータベースID
    /// </summary>
    /// <value></value>
    public string NotionDatabaseId { get; private set; } = string.Empty;

    /// <summary>
    /// 出力ディレクトリのパステンプレート
    /// </summary>
    /// <value></value>
    public string OutputDirectoryPathTemplate { get; private set; } = string.Empty;

    /// <summary>
    /// コマンドライン引数から設定を読み込む
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static AppConfiguration FromCommandLine(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentException("Required arguments: [NotionAuthToken] [DatabaseId] [OutputPathTemplate]");
        }

        return new AppConfiguration
        {
            NotionAuthToken = args[0],
            NotionDatabaseId = args[1],
            OutputDirectoryPathTemplate = args[2]
        };
    }
}
