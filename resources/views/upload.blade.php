<form action="{{ route('api.upload') }}" method="POST" enctype="multipart/form-data">

    <input type="file" name="file">

    <input type="submit" value="Upload">

</form>