<?php 

$path = './Data';
$files = scandir($path);

#echo count($files);
$count = 1;
foreach ($files as &$file)
{
    if ('.' !== $file && '..' !== $file)
    {
        if($count == count($files))
        {
            echo $file;
        }
        else
        {
            echo $file . " ";
        }
    }
    $count = $count + 1;
}

?>