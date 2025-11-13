event_inherited()
with (o_player)
{
    scr_atr_incr("SP", 1)
    scr_guiAnimation(15885, 1, 1)
    audio_play_sound(snd_altar_buff, 4, 0)
	scr_voice_text(choose("Holy Hell!", "Amen.", "*gasps*"))
}
