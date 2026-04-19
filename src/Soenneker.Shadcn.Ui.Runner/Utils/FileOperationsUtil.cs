using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Soenneker.Git.Util.Abstract;
using Soenneker.Node.Util.Abstract;
using Soenneker.Playwrights.Crawler.Abstract;
using Soenneker.Playwrights.Crawler.Dtos;
using Soenneker.Playwrights.Crawler.Enums;
using Soenneker.Shadcn.Ui.Runner.Utils.Abstract;
using Soenneker.Utils.Directory.Abstract;
using Soenneker.Utils.Environment;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Path.Abstract;
using Soenneker.Utils.Process.Abstract;

namespace Soenneker.Shadcn.Ui.Runner.Utils;

///<inheritdoc cref="IFileOperationsUtil"/>
public sealed class FileOperationsUtil : IFileOperationsUtil
{
    private readonly ILogger<FileOperationsUtil> _logger;
    private readonly IGitUtil _gitUtil;
    private readonly INodeUtil _nodeUtil;
    private readonly IPlaywrightCrawler _playwrightCrawler;
    private readonly IPathUtil _pathUtil;
    private readonly IDirectoryUtil _directoryUtil;
    private readonly IFileUtil _fileUtil;
    private readonly IProcessUtil _processUtil;

    public FileOperationsUtil(ILogger<FileOperationsUtil> logger, IGitUtil gitUtil, INodeUtil nodeUtil, IPlaywrightCrawler playwrightCrawler,
        IPathUtil pathUtil, IDirectoryUtil directoryUtil, IFileUtil fileUtil, IProcessUtil processUtil)
    {
        _logger = logger;
        _gitUtil = gitUtil;
        _nodeUtil = nodeUtil;
        _playwrightCrawler = playwrightCrawler;
        _pathUtil = pathUtil;
        _directoryUtil = directoryUtil;
        _fileUtil = fileUtil;
        _processUtil = processUtil;
    }

    public async ValueTask Process(CancellationToken cancellationToken)
    {
        string tempRoot = await _pathUtil.GetUniqueTempDirectory("soenneker-shadcn-ui-runner", true, cancellationToken);
        string shadcnUiDirectory = Path.Combine(tempRoot, "shadcn-ui");
        //string shadcnDocsDirectory = Path.Combine(shadcnUiDirectory, "apps", "v4");
        string crawledRepositoryDirectory = Path.Combine(tempRoot, "soenneker-shadcn-ui-crawled");
        string componentsRepositoryDirectory = Path.Combine(tempRoot, "soenneker-shadcn-ui-components");
        string crawlDirectory = Path.Combine(tempRoot, "crawl");
        string extractedDirectory = Path.Combine(tempRoot, "extracted");

        await CloneRepository(Constants.CrawledRepository, crawledRepositoryDirectory, cancellationToken);
        await CloneRepository(Constants.ComponentsRepository, componentsRepositoryDirectory, cancellationToken);


        // Will wait on local crawling until they fix the ability to actually build it consistently...
        //await CloneRepository(Constants.ShadcnUiRepository, shadcnUiDirectory, cancellationToken);

        //await _nodeUtil.EnsureInstalled(cancellationToken: cancellationToken);

        //await _nodeUtil.InstallPnpm(false, cancellationToken);
        //await _nodeUtil.RunNpmCommand("install -g bun", cancellationToken);

        //string pnpmPath = await _nodeUtil.GetPnpmPath(cancellationToken);

        //await RenameEnvExample(shadcnDocsDirectory, cancellationToken);

        //await RunProcess(pnpmPath, "install", shadcnUiDirectory, cancellationToken, TimeSpan.FromMinutes(10));
        //await RunProcess(pnpmPath, "build", shadcnUiDirectory, cancellationToken, TimeSpan.FromMinutes(10));


        try
        {
           // await WaitForSite(Constants.ComponentsUrl, cancellationToken);

            await Crawl(crawlDirectory, cancellationToken);
            await ReplaceRepositoryContents(crawledRepositoryDirectory, crawlDirectory, cancellationToken);
            await CommitAndPush(crawledRepositoryDirectory, cancellationToken);
            await ExtractPreviewFamilies(crawlDirectory, extractedDirectory, cancellationToken);
            await ReplaceRepositoryContents(componentsRepositoryDirectory, extractedDirectory, cancellationToken);
            await CommitAndPush(componentsRepositoryDirectory, cancellationToken);
        }
        finally
        {
            try
            {
             //   if (!devServer.HasExited)
             //       await _processUtil.Kill(devServer, true, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not stop pnpm dev cleanly.");
            }
        }
    }

    private async ValueTask CloneRepository(string repositoryUrl, string targetDirectory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cloning {RepositoryUrl} into {TargetDirectory}", repositoryUrl, targetDirectory);
        await _gitUtil.Clone(repositoryUrl, targetDirectory, cancellationToken: cancellationToken);
    }

    //private async ValueTask RenameEnvExample(string shadcnDocsDirectory, CancellationToken cancellationToken)
    //{
    //    string source = Path.Combine(shadcnDocsDirectory, ".env.example");
    //    string destination = Path.Combine(shadcnDocsDirectory, ".env");

    //    if (!await _fileUtil.Exists(source, cancellationToken))
    //    {
    //        _logger.LogWarning("Expected env example file does not exist at {Path}", source);
    //        return;
    //    }

    //    await _fileUtil.DeleteIfExists(destination, cancellationToken: cancellationToken);

    //    await _fileUtil.Copy(source, destination, cancellationToken: cancellationToken);
    //}

    //private async ValueTask<Process> StartDevServer(string shadcnUiDirectory, string pnpmPath,
    //    CancellationToken cancellationToken)
    //{
    //    var dto = new ProcessStartDto
    //    {
    //        FileName = pnpmPath,
    //        Arguments = "--filter=v4 exec next dev --turbopack --port 4000",
    //        WorkingDirectory = shadcnUiDirectory,
    //        CreateNoWindow = true,
    //        Log = true
    //    };

    //    Process? process = await _processUtil.StartDetached(dto, cancellationToken);

    //    if (process == null)
    //        throw new InvalidOperationException("Could not start pnpm next dev.");

    //    _logger.LogInformation("Started pnpm next dev");

    //    return process;
    //}

    //private async ValueTask WaitForSite(string url, CancellationToken cancellationToken)
    //{
    //    _logger.LogInformation("Waiting for site to compile...");

    //    using var httpClient = new HttpClient();
    //    httpClient.Timeout = TimeSpan.FromSeconds(60);

    //    DateTimeOffset deadline = DateTimeOffset.UtcNow.AddMinutes(5);

    //    while (DateTimeOffset.UtcNow < deadline)
    //    {
    //        cancellationToken.ThrowIfCancellationRequested();

    //        try
    //        {
    //            using HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

    //            if (response.StatusCode == HttpStatusCode.OK)
    //                return;
    //        }
    //        catch
    //        {
    //            // Ignore until timeout.
    //        }

    //        await DelayUtil.Delay(TimeSpan.FromSeconds(2), _logger, cancellationToken);
    //    }

    //    throw new TimeoutException($"Timed out waiting for {url}");
    //}

    private async ValueTask Crawl(string crawlDirectory, CancellationToken cancellationToken)
    {
        PlaywrightCrawlResult result = await _playwrightCrawler.Crawl(new PlaywrightCrawlOptions
        {
            Url = Constants.ComponentsUrl,
            SaveDirectory = crawlDirectory,
            Mode = PlaywrightCrawlMode.Full,
            MaxDepth = 10,
            MaxPages = 500,
            SameHostOnly = true,
            PrettyPrintHtml = true,
            ClearSaveDirectory = true,
            OverwriteExistingFiles = true,
            ContinueOnPageError = true,
            Headless = true,
            UseStealth = true,
            NavigationTimeoutMs = 60_000,
            PostNavigationDelayMs = 2_000
        }, cancellationToken);

        _logger.LogInformation("Crawl complete. PagesVisited: {PagesVisited}, HtmlFilesSaved: {HtmlFilesSaved}", result.PagesVisited, result.HtmlFilesSaved);
    }

    private async ValueTask ExtractPreviewFamilies(string crawlDirectory, string extractedDirectory, CancellationToken cancellationToken)
    {
        await _directoryUtil.DeleteIfExists(extractedDirectory, cancellationToken);

        await _directoryUtil.Create(extractedDirectory, cancellationToken: cancellationToken);

        string componentsRoot = Path.Combine(crawlDirectory, "docs", "components");

        if (!await _directoryUtil.Exists(componentsRoot, cancellationToken))
            throw new DirectoryNotFoundException($"Expected components crawl directory was not found: {componentsRoot}");

        var parser = new HtmlParser();
        var formatter = new PrettyMarkupFormatter();

        List<string> htmlFiles = await _directoryUtil.GetFilesByExtension(componentsRoot, ".html", true, cancellationToken);

        foreach (string htmlFile in htmlFiles.OrderBy(static path => path, StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            string html = await _fileUtil.Read(htmlFile, cancellationToken: cancellationToken);
            IDocument document = await parser.ParseDocumentAsync(html, cancellationToken);
            IHtmlCollection<IElement> previews = document.QuerySelectorAll("div[data-slot=\"preview\"]");

            if (previews.Length == 0)
                continue;

            string familyName = GetFamilyName(componentsRoot, htmlFile);
            string outputPath = Path.Combine(extractedDirectory, familyName + ".html");

            var builder = new StringBuilder();
            builder.AppendLine("<!DOCTYPE html>");
            builder.AppendLine("<html>");
            builder.AppendLine("  <body>");

            foreach (IElement preview in previews)
            {
                await using var writer = new StringWriter();
                preview.ToHtml(writer, formatter);
                builder.AppendLine(IndentBlock(writer.ToString().Trim(), "    "));
                builder.AppendLine();
            }

            builder.AppendLine("  </body>");
            builder.AppendLine("</html>");

            await _fileUtil.Write(outputPath, builder.ToString().TrimEnd() + Environment.NewLine, cancellationToken: cancellationToken);
            _logger.LogInformation("Saved {Count} previews to {Path}", previews.Length, outputPath);
        }
    }

    private async ValueTask ReplaceRepositoryContents(string repositoryDirectory, string sourceDirectory, CancellationToken cancellationToken)
    {
        List<string> directories = await _directoryUtil.GetAllDirectories(repositoryDirectory, cancellationToken);

        foreach (string directory in directories)
        {
            if (Path.GetFileName(directory).Equals(".git", StringComparison.OrdinalIgnoreCase))
                continue;

            await _directoryUtil.Delete(directory, cancellationToken);
        }

        List<string> files = await _directoryUtil.GetFilesByExtension(repositoryDirectory, "", false, cancellationToken);

        foreach (string file in files)
        {
            await _fileUtil.Delete(file, cancellationToken: cancellationToken);
        }

        List<string> sourcePaths = await _directoryUtil.GetFilesByExtension(sourceDirectory, "", true, cancellationToken);

        foreach (string sourcePath in sourcePaths)
        {
            string relativePath = Path.GetRelativePath(sourceDirectory, sourcePath);
            string destination = Path.Combine(repositoryDirectory, relativePath);
            string? destinationDirectory = Path.GetDirectoryName(destination);

            if (!string.IsNullOrWhiteSpace(destinationDirectory))
                await _directoryUtil.Create(destinationDirectory, cancellationToken: cancellationToken);

            await _fileUtil.DeleteIfExists(destination, cancellationToken: cancellationToken);
            await _fileUtil.Copy(sourcePath, destination, cancellationToken: cancellationToken);
        }
    }

    private async ValueTask CommitAndPush(string repositoryDir, CancellationToken cancellationToken)
    {
        string token = EnvironmentUtil.GetVariableStrict("GH__TOKEN");
        string? name = EnvironmentUtil.GetVariableStrict("GIT__NAME");
        string? email = EnvironmentUtil.GetVariableStrict("GIT__EMAIL");

        await _gitUtil.CommitAndPush(repositoryDir, Constants.CommitMessage, token, name, email, cancellationToken);
    }

    private async ValueTask<string> RunProcess(string fileName, string arguments, string workingDirectory, CancellationToken cancellationToken, TimeSpan timeout,
        bool captureOutput = false)
    {
        _logger.LogInformation("Running: {FileName} {Arguments}", fileName, arguments);
        List<string> lines = await _processUtil.Start(fileName, workingDirectory, arguments, waitForExit: true, timeout: timeout, log: true,
            cancellationToken: cancellationToken);

        return captureOutput ? string.Join(Environment.NewLine, lines) : string.Empty;
    }

    private static string GetFamilyName(string componentsRoot, string htmlFile)
    {
        string relativePath = Path.GetRelativePath(componentsRoot, htmlFile);
        string withoutExtension = Path.ChangeExtension(relativePath, null) ?? relativePath;
        string normalized = withoutExtension.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

        if (normalized.EndsWith("-index", StringComparison.OrdinalIgnoreCase))
            normalized = normalized[..^"-index".Length];

        return normalized.Equals("index", StringComparison.OrdinalIgnoreCase) ? "components" : normalized;
    }

    private static string IndentBlock(string content, string indent)
    {
        string[] lines = content.Replace("\r\n", "\n").Split('\n');
        return string.Join(Environment.NewLine, lines.Select(line => indent + line));
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
