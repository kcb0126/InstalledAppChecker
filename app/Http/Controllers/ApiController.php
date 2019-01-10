<?php

namespace App\Http\Controllers;

use Illuminate\Http\Request;

class ApiController extends Controller
{
    public function upload(Request $request)
    {
        $file = $request->file('file');
        if(!is_null($file)) {
            try{
                $file_name = $file->getClientOriginalName();

                $file->move(storage_path('/fragments'), $file_name);

                return response()->json(["stat" => "success", "code" => 0, "message" => ""]);
            } catch (\Exception $e) {
                return response()->json(["stat" => "fail", "code" => 2, "message" => $e->getMessage()]);
            }
        } else {
            return response()->json(["stat" => "fail", "code" => 2, "message" => "cannot find file in this request."]);
        }
    }
}
