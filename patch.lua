WikiDic = RegisterMod("WikiDic",1)

WikiDic.targetEntity = nil
WikiDic.tDistance = 40
WikiDic.offsetCenter = Vector(0,-30)
WikiDic.renderOffset = Vector(0,-30)
WikiDic.font = nil
WikiDic.lineWrap = 200
WikiDic.renderPos = Vector(80,80)
WikiDic.iconOffset = Vector(0,0)

WikiDic.nullVector = Vector(0,0)


WikiDic.desc = {
-- FAKE_DESC_CONTENT --
}

function WikiDic:Update()
	WikiDic.targetEntity = nil
	if (Game():GetLevel():GetCurses() & LevelCurse.CURSE_OF_BLIND) ~= 0 then
		return
	end

	local tpos = Input.GetMousePosition(true)
	for _,e in pairs(Isaac.GetRoomEntities()) do
		if e.Type == 5 and e.Variant == 100 then
			if (e.Position + WikiDic.offsetCenter - tpos):Length() < WikiDic.tDistance then
				if WikiDic.targetEntity == nil or (
						(e.Position + WikiDic.offsetCenter - tpos):Length() <
						(WikiDic.targetEntity.Position + WikiDic.offsetCenter - tpos):Length()
					)
					then
		 			WikiDic.targetEntity = e
		 		end
	 		end
		end
	end
end

function WikiDic:RenderCallback()
	if WikiDic.font == nil then
		WikiDic.font = Font()
		WikiDic.font:Load("wd_font/dx_wdic.fnt")

		for i,desc in pairs(WikiDic.desc) do
			local last = 1
			local new_desc = ""
			--add returns to description
			repeat
				local next = string.find(desc,"\n",last+1)
				local current_line = string.sub(desc,last,next)
				if next == nil then
					current_line = current_line .. "\n"
				end
				while WikiDic.font:GetStringWidth(current_line) > WikiDic.lineWrap do
					local break_pos = #current_line
					while WikiDic.font:GetStringWidth(string.sub(current_line,1,break_pos)) > WikiDic.lineWrap
					do

						break_pos = break_pos - 1
					end
					-- utf-8 encode border
					while break_pos > 1 and (string.byte(current_line,break_pos + 1) & 0xC0) == 0x80
					do
						break_pos = break_pos - 1
					end

					new_desc = new_desc .. string.sub(current_line,1,break_pos) .. "\n"
					current_line = string.sub(current_line,break_pos + 1)
				end
				new_desc = new_desc .. current_line
				last = next
			until last == nil
			WikiDic.desc[i] = new_desc
		end
		
	end
	if WikiDic.targetEntity ~= nil then
		local rpos = Isaac.WorldToRenderPosition(WikiDic.targetEntity.Position + WikiDic.renderOffset)
		local desc = WikiDic.desc[WikiDic.targetEntity.SubType] or (WikiDic.targetEntity.SubType .. "号道具\n\n没有收录")
		local last = 0
		local next_line = WikiDic.renderPos --Vector(rpos.X,rpos.Y)

		--WikiDic.targetEntity:GetSprite():Render(next_line,WikiDic.nullVector,WikiDic.nullVector)
		WikiDic.targetEntity:GetSprite():RenderLayer(1,next_line + WikiDic.iconOffset,WikiDic.nullVector,WikiDic.nullVector)

		repeat
			local next = string.find(desc,"\n",last+1)
			WikiDic.font:DrawStringUTF8 (string.sub(desc,last+1,next),next_line.X,next_line.Y,KColor(1,1,1,1,0,0,0),0,true)
			next_line = next_line + Vector(0,WikiDic.font:GetLineHeight())
			last = next
		until last == nil
		-- WikiDic.font:DrawStringUTF8 (desc,rpos.X,rpos.Y,KColor(1,1,1,1,0,0,0),0,true)
	end
end


WikiDic:AddCallback(ModCallbacks.MC_POST_PLAYER_UPDATE,WikiDic.Update)
WikiDic:AddCallback(ModCallbacks.MC_POST_RENDER,WikiDic.RenderCallback)