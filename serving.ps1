
$source = New-Object System.Text.StringBuilder

Get-ChildItem |
    Where-Object { $_.Name -match '\.cs$' } |
    ForEach-Object { Get-Content -Path $_ -Raw } |
    ForEach-Object { $source.AppendLine($_) > $null }

$addTypeParams = @{
    TypeDefinition = $source.ToString()
    Language = 'CSharpVersion3'
    ReferencedAssemblies = 'System.Windows.Forms', 'System.Drawing'
}

Add-Type @addTypeParams

try {
    $server = New-Object HttpListening.CheapHttpServer
    $text = @'
http://localhost:{0}
{1}
'@ -f $server.Port, $server.ContentRoot
    $server.Start()
    [System.Windows.Forms.MessageBox]::Show($text)
} finally {
    if ($null -ne $server){
        $server.Close()
        $server = $null
    }
}
