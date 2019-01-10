<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;
use Illuminate\Support\Facades\File;

class FilesController extends Controller
{
    public function index()
    {
        $links = [];

        $uploadedFiles = File::files(public_path('/uploads'));
        foreach ($uploadedFiles as $file) {
            $links[] = url('uploads/' . $file->getFilename());
        }

        while(count($fragments = File::files(storage_path('/fragments'))) > 0)
        {
            $picked = $fragments[0];

            $file_name = $picked->getFilename();

            try {
                $end_string_pos = strrpos($file_name, "__");
                $end_string = substr($file_name, $end_string_pos + 2);
                $origin_file_name = substr($file_name, 0, $end_string_pos);

                $count_string_pos = strpos($end_string, "_");

                $count = (int)substr($end_string, 0, $count_string_pos);

                $target = fopen(public_path('/uploads/' . $origin_file_name), 'w');
                for($index = 0; $index < $count; $index++) {
                    $frgmnt = file_get_contents(storage_path('/fragments/' . $origin_file_name . '__' . (string)$count . '_' . (string)$index));
                    fwrite($target, $frgmnt);
                    File::delete(storage_path('/fragments/' . $origin_file_name . '__' . (string)$count . '_' . (string)$index));
                }
                $links[] = url('uploads/' . $origin_file_name);
                fclose($target);
            } catch(\Exception $exception) {
                File::delete($picked->getPathname());
                continue;
            }
        }

        return view('list')->with('links', $links);
    }
}
