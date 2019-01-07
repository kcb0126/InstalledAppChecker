<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;

class ApiController extends Controller
{
    public function upload(Request $request)
    {
        $file = $request->file('file');
        if(!is_null($file)) {
            $file->move(public_path('/uploads'), $file->getClientOriginalName());
        }
    }
}
