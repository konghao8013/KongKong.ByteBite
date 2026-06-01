using System.Reflection;
using ByteBite.Api.Controllers;
using ByteBite.Application.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ByteBite.IntegrationTests.Current;

public class FilesControllerTests
{
    private const long MaxUploadBytes = 5 * 1024 * 1024;

    [Fact]
    public async Task Upload_AcceptsFileAtDocumentedLimit()
    {
        var webRoot = Path.Combine(Path.GetTempPath(), "bytebite-upload-tests", Guid.NewGuid().ToString("N"));
        var controller = new FilesController(CreateEnvironment(webRoot));

        try
        {
            var result = await controller.Upload(CreateFormFile(MaxUploadBytes), CancellationToken.None);

            result.GetType().GetProperty("Size")!.GetValue(result).Should().Be(MaxUploadBytes);
            Directory.GetFiles(webRoot, "*.jpg", SearchOption.AllDirectories).Should().ContainSingle();
        }
        finally
        {
            if (Directory.Exists(webRoot))
            {
                Directory.Delete(webRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Upload_RejectsFileAboveDocumentedLimit()
    {
        var controller = new FilesController(CreateEnvironment());

        var act = () => controller.Upload(CreateFormFile(MaxUploadBytes + 1), CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be(400);
        exception.Which.Message.Should().Contain("5MB");
    }

    [Fact]
    public void Upload_RequestBodyLimitLeavesRoomForMultipartEnvelope()
    {
        var method = typeof(FilesController).GetMethod(nameof(FilesController.Upload))!;
        var requestSizeLimit = method.GetCustomAttribute<RequestSizeLimitAttribute>();
        var formLimits = method.GetCustomAttribute<RequestFormLimitsAttribute>();

        requestSizeLimit.Should().NotBeNull();
        ((IRequestSizeLimitMetadata)requestSizeLimit!).MaxRequestBodySize.Should().BeGreaterThan(MaxUploadBytes);
        formLimits.Should().NotBeNull();
        formLimits!.MultipartBodyLengthLimit.Should().BeGreaterThan(MaxUploadBytes);
    }

    private static FormFile CreateFormFile(long size)
    {
        var content = new byte[(int)size];
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, size, "file", "dish.jpg");
    }

    private static IWebHostEnvironment CreateEnvironment(string? webRoot = null)
    {
        var root = webRoot ?? Path.Combine(Path.GetTempPath(), "bytebite-upload-tests", Guid.NewGuid().ToString("N"));
        return new TestWebHostEnvironment
        {
            ContentRootPath = root,
            WebRootPath = root,
        };
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ByteBite.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Development;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
    }
}
