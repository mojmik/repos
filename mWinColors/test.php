<?php
echo "<br>hi";

$paths = [
    '/opt/365id-php/logs/decrypted*.json',
    '/opt/365id-php/logs/encrypted*.pgp',
    '/opt/365id-php/logs/gpg_out*.log',
    '/opt/365id-php/logs/scan*.log',
    '/opt/365id-php/storage/pending*.json',
];

$maxAge = 24 * 3600; // 1 day in seconds
$now = time();

foreach ($paths as $pattern) {
	echo "<br>$pattern";
    foreach (glob($pattern) as $file) {
		echo "<br>$file";
        if (is_file($file)) {
            $age = $now - filemtime($file);
            if ($age > $maxAge) {
				 if (!@unlink($file)) {
					$err = error_get_last();
					$msg = $err['message'] ?? 'unknown error';
					echo "<br>CLEANUP: unlink FAILED for $file (age={$age}s, mtime=" . date('c', $mtime) . "): $msg";
				} else {
					echo "<br>CLEANUP: deleted $file (age={$age}s, mtime=" . date('c', $mtime) . ")";
				}
            }
        }
    }
}

echo "<br>done";