WikiDic = RegisterMod("WikiDic",1)

WikiDic.targetEntity = nil
WikiDic.tDistance = 40
WikiDic.offsetCenter = Vector(0,-30)
WikiDic.renderOffset = Vector(0,-30)
WikiDic.font = nil
WikiDic.lineWrap = 200
WikiDic.renderPos = Vector(80,50)
WikiDic.iconNoShopItemOffset = Vector(-12,30)
WikiDic.iconOffset = Vector(-12,15)
WikiDic.trinketIconOffset = Vector(-15,15)
WikiDic.cursur_sprite = nil

WikiDic.nullVector = Vector(0,0)

-- FAKE_CONFIG_SEG_1 --

--WikiDic.usePlayerPos = true --mouse or player pos
--WikiDic.drawMouse = true

if WikiDic.usePlayerPos then
	WikiDic.tDistance = 100
end


WikiDic.desc = {
-- FAKE_DESC_CONTENT --
}

WikiDic.trinketDesc = {
-- FAKE_TRINKET_CONTENT --
}

function WikiDic:Update()
	WikiDic.targetEntity = nil
	-- if (Game():GetLevel():GetCurses() & LevelCurse.CURSE_OF_BLIND) ~= 0 then
	-- 	return
	-- end

	local tpos = WikiDic.usePlayerPos and Isaac.GetPlayer(0).Position or Input.GetMousePosition(true)
	for _,e in pairs(Isaac.GetRoomEntities()) do
		if e.Type == 5 and (e.Variant == 100 or e.Variant == 350) then
			if e.Variant == 100 and (e.SubType == 0 or (Game():GetLevel():GetCurses() & LevelCurse.CURSE_OF_BLIND) ~= 0) then
				-- do nothing in CURSE_OF_BLIND
			else
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
end

function WikiDic:FixReturn(descs)
	for i,desc in pairs(descs) do
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
		descs[i] = new_desc
	end

end

function WikiDic:RenderCallback()
	if WikiDic.font == nil then
		WikiDic.font = Font()
		WikiDic.font:Load("wd_font/dx_wdic.fnt")
		WikiDic:FixReturn(WikiDic.desc)
		WikiDic:FixReturn(WikiDic.trinketDesc)
	end

	if WikiDic.drawMouse then
		if WikiDic.cursur_sprite == nil then
			WikiDic.cursur_sprite = Sprite()
			WikiDic.cursur_sprite:Load("gfx/ui/cursor.anm2",true)
		end
		WikiDic.cursur_sprite:Play(WikiDic.targetEntity and "Clicked" or "Idle",true)
		local mousePos = Isaac.WorldToScreen(Input.GetMousePosition(true))
		WikiDic.cursur_sprite:Render(mousePos,WikiDic.nullVector,WikiDic.nullVector)
	end

	if WikiDic.targetEntity ~= nil then
		local rpos = Isaac.WorldToRenderPosition(WikiDic.targetEntity.Position + WikiDic.renderOffset)
		local desc = nil
		if WikiDic.targetEntity.Variant == 100 then
			desc = WikiDic.desc[WikiDic.targetEntity.SubType] or (WikiDic.targetEntity.SubType .. "号道具\n\n没有收录")
		end
		if WikiDic.targetEntity.Variant == 350 then
			desc = WikiDic.trinketDesc[WikiDic.targetEntity.SubType] or (WikiDic.targetEntity.SubType .. "号饰品\n\n没有收录")
		end

		local last = 0
		local next_line = WikiDic.renderPos --Vector(rpos.X,rpos.Y)

		if WikiDic.targetEntity.Variant == 100 then
			--WikiDic.targetEntity:GetSprite():Render(next_line,WikiDic.nullVector,WikiDic.nullVector)
			local icon_offset = (not WikiDic.targetEntity:ToPickup():IsShopItem()) and WikiDic.iconNoShopItemOffset or WikiDic.iconOffset
			WikiDic.targetEntity:GetSprite():RenderLayer(1,next_line + icon_offset,WikiDic.nullVector,WikiDic.nullVector)
		end
		if WikiDic.targetEntity.Variant == 350 then
			local icon_offset = WikiDic.trinketIconOffset
			WikiDic.targetEntity:GetSprite():RenderLayer(0,next_line + icon_offset,WikiDic.nullVector,WikiDic.nullVector)
		end
		repeat
			local next = string.find(desc,"\n",last+1)
			WikiDic.font:DrawStringUTF8 (string.sub(desc,last+1,next),next_line.X,next_line.Y,KColor(1,1,1,0.6,0,0,0),0,true)
			next_line = next_line + Vector(0,WikiDic.font:GetLineHeight())
			last = next
		until last == nil
		-- WikiDic.font:DrawStringUTF8 (desc,rpos.X,rpos.Y,KColor(1,1,1,1,0,0,0),0,true)
	end
end


WikiDic:AddCallback(ModCallbacks.MC_POST_PLAYER_UPDATE,WikiDic.Update)
WikiDic:AddCallback(ModCallbacks.MC_POST_RENDER,WikiDic.RenderCallback)
