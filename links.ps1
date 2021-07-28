
$item = '<li><a href="{0}">{0}</a></li>'

Get-ChildItem |
    Where-Object { $_.PSIsContainer } |
    ForEach-Object { $_.Name + '/' } |
    ForEach-Object { $item -f $_ }

Get-ChildItem |
    ForEach-Object { $_.Name } |
    ForEach-Object { $item -f $_ }