function scr_zcustomization_generator() //gml_Script_scr_zcustomization_generator
{
    if !is_array(argument[0])
        var _array = scr_tableLoad(argument[0])
    else
        var _array = argument[0]
    var _arrayLength = array_length(_array[0])
    var _gen_array = []
    for (var _i = 0; _i < _arrayLength; _i++)
    {
        var _trigger = false
        for (var _j = 1; _j <= argument_count; _j += 2)
        {
            if (_array[0][_i] == argument[_j])
            {
                array_push(_gen_array, argument[(_j + 1)])
                _trigger = true
            }
        }
        if !_trigger
            array_push(_gen_array, "")
    }
    return _gen_array;
}

