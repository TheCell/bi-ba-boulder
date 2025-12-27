<?php

namespace App\ImageModification;

use Symfony\Component\Filesystem\Filesystem;
use Symfony\Component\Uid\Uuid;

final class ImageModification
{
    public static function getSpraywallProblemImage(Uuid $spraywallId, Uuid $spraywallProblemId, Filesystem $filesystem): string {
        $imagePath = 'spraywalls' . DIRECTORY_SEPARATOR . $spraywallId . DIRECTORY_SEPARATOR . $spraywallProblemId . '.png';
        $contents = $filesystem->readFile($imagePath);
        return base64_encode($contents);
    }

    public static function compressAndConvertTo24BitPng(string $binaryData): string {
        if (!extension_loaded('gd')) {
            throw new \RuntimeException('GD extension is not available. Please install php-gd.');
        }
    
        try {
            $sourceImage = imagecreatefromstring($binaryData);
            
            if ($sourceImage === false) {
                throw new \InvalidArgumentException('Invalid image data - could not create image from string');
            }
            
            $width = 128;
            $height = 128;
            
            // Create a new 24-bit image (without alpha channel)
            $newImage = imagecreatetruecolor($width, $height);
            
            if ($newImage === false) {
                $sourceImage = null;
                throw new \RuntimeException('Failed to create new image canvas');
            }
            
            // Disable alpha blending for the destination image
            imagealphablending($newImage, false);
            
            $backgroundColor = imagecolorallocate($newImage, 0, 0, 0);
            
            if ($backgroundColor === false) {
                $sourceImage = null;
                $newImage = null;
                throw new \RuntimeException('Failed to allocate background color');
            }
            
            imagefill($newImage, 0, 0, $backgroundColor);
            
            // Enable alpha blending for copying the source image
            imagealphablending($newImage, true);
            
            // Copy the source image onto the new image
            // This will flatten any transparency against the background color
            $copyResult = imagecopy($newImage, $sourceImage, 0, 0, 0, 0, $width, $height);
            
            if (!$copyResult) {
                $sourceImage = null;
                $newImage = null;
                throw new \RuntimeException('Failed to copy source image to new canvas');
            }
            
            // Capture the PNG output using imagepng
            ob_start();
            $pngResult = imagepng($newImage, null, 9); // 9 = maximum compression level
            
            if (!$pngResult) {
                ob_end_clean();
                $sourceImage = null;
                $newImage = null;
                throw new \RuntimeException('Failed to generate PNG output');
            }
            
            $processedImageData = ob_get_contents();
            ob_end_clean();
            
            // Clean up memory
            $sourceImage = null;
            $newImage = null;
            
            if ($processedImageData === false || empty($processedImageData)) {
                throw new \RuntimeException('Generated PNG data is empty or invalid');
            }
            
            return $processedImageData;
            
        } catch (\Exception $e) {
            throw new \RuntimeException('Failed to process image with GD: ' . $e->getMessage());
        }
    }
}
