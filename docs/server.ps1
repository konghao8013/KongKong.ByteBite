$listener = [System.Net.HttpListener]::new()
$listener.Prefixes.Add('http://localhost:8080/')
$listener.Start()
Write-Host 'Server running at http://localhost:8080/style-guide.html'
while ($listener.IsListening) {
    $ctx = $listener.GetContext()
    $path = $ctx.Request.Url.LocalPath.TrimStart('/')
    if ($path -eq '') { $path = 'style-guide.html' }
    $fullPath = Join-Path 'G:\code\KongKong.ByteBite\docs' $path
    if (Test-Path $fullPath -PathType Leaf) {
        $content = [System.IO.File]::ReadAllBytes($fullPath)
        $ext = [System.IO.Path]::GetExtension($fullPath)
        if ($ext -eq '.html') { $ctx.Response.ContentType = 'text/html; charset=utf-8' }
        elseif ($ext -eq '.css') { $ctx.Response.ContentType = 'text/css' }
        elseif ($ext -eq '.js') { $ctx.Response.ContentType = 'application/javascript' }
        else { $ctx.Response.ContentType = 'application/octet-stream' }
        $ctx.Response.ContentLength64 = $content.Length
        $ctx.Response.OutputStream.Write($content, 0, $content.Length)
    } else {
        $ctx.Response.StatusCode = 404
    }
    $ctx.Response.Close()
}
