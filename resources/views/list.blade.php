<table>
    <tr>
        <th>No</th>
        <th>File</th>
    </tr>

    @foreach($links as $index => $link)

        <tr>
            <td>{{ $index + 1 }}</td>
            <td><a href="{{$link}}" target="_blank">{{$link}}</a></td>
        </tr>

    @endforeach

</table>