function scr_zcustomization_textloader() //gml_Script_scr_zcustomization_textloader
{
    ds_map_add(global.consum_name, "zspscroll", "Skill Point Scroll")
    ds_map_add(global.consum_desc, "zspscroll", "Grants 1 skill point when use")

    ds_map_add(global.consum_name, "zapscroll", "Stat Point Scroll")
    ds_map_add(global.consum_desc, "zapscroll", "Grants 1 stat point when use")

    array_push(global.consum_csv, scr_zcustomization_generator(global.consum_csv, "id", "zspscroll", "Price", "9000", "Cat", "scroll", "Subcat", "none", "Material", "paper", "Weight", "Light", "tags", "rare"))
    array_push(global.consum_csv, scr_zcustomization_generator(global.consum_csv, "id", "zapscroll", "Price", "3000", "Cat", "scroll", "Subcat", "none", "Material", "paper", "Weight", "Light", "tags", "rare"))
    scr_array2d_to_map(global.consum_csv, global.consum_stat_data, global.consum_string_attribute)
}
