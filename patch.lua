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
WikiDic.iconScale = Vector(1,1)
WikiDic.fontScale = 1
WikiDic.distYMulti = 1

WikiDic.nullVector = Vector(0,0)

WikiDic.usePlayerPos = false
WikiDic.drawMouse = true
WikiDic.useDefaultFont = false

-- FAKE_CONFIG_SEG_1 --

--WikiDic.usePlayerPos = true --mouse or player pos
--WikiDic.drawMouse = true
--WikiDic.useDefaultFont = true
--WikiDic.useHalfSizeFont = true
--WikiDic.useBiggerSizeFont = true

if WikiDic.usePlayerPos then
	WikiDic.tDistance = 180
	WikiDic.offsetCenter = Vector(0,0)
	WikiDic.distYMulti = 1.6 --y is far than screen distance
end

if WikiDic.useHalfSizeFont then
	WikiDic.renderPos = Vector(70,45)
	WikiDic.iconNoShopItemOffset = Vector(-8,17)
	WikiDic.iconOffset = Vector(-8,8)
	WikiDic.trinketIconOffset = Vector(-8,6)
	WikiDic.iconScale = Vector(0.5,0.5)
	WikiDic.fontScale = 0.5
end
if WikiDic.useBiggerSizeFont then
	WikiDic.renderPos = Vector(80,45)
	WikiDic.iconNoShopItemOffset = Vector(-8,40)
	WikiDic.iconOffset = Vector(-15,20)
	WikiDic.trinketIconOffset = Vector(-10,12)
	WikiDic.iconScale = Vector(1.2,1.2)
	WikiDic.fontScale = 1.2
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
				local distVec = (e.Position + WikiDic.offsetCenter - tpos)
				distVec.Y = distVec.Y * WikiDic.distYMulti
				if distVec:Length() < WikiDic.tDistance then
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
	local byte_0 = string.byte("0")
	local byte_9 = string.byte("9")
	local byte_a = string.byte("a")
	local byte_z = string.byte("z")
	local byte_A = string.byte("A")
	local byte_Z = string.byte("Z")
	local byte_period = string.byte(".")
	local byte_comma = string.byte(",")
	local byte_space = string.byte(" ")
	local is_word = function(b)
		return (byte_0 <= b and b <= byte_9) or (byte_A <= b and b <= byte_Z) or (byte_a <= b and b <= byte_z) or b == byte_comma or b == byte_period
	end
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
				-- don't break at word or number end
				if is_word(string.byte(current_line,break_pos)) then
					while break_pos < #current_line and is_word(string.byte(current_line,break_pos + 1)) do
						break_pos = break_pos + 1
					end
				end
				--remove start space
				local new_line_begin = 1
				while new_line_begin < break_pos and string.byte(current_line,new_line_begin) == byte_space do
					new_line_begin = new_line_begin + 1
				end
				new_desc = new_desc .. string.sub(current_line,new_line_begin,break_pos) .. "\n"
				current_line = string.sub(current_line,break_pos + 1)
			end
			--remove start space
			local new_line_begin = 1
			while new_line_begin < #current_line and string.byte(current_line,new_line_begin) == byte_space do
				new_line_begin = new_line_begin + 1
			end

			new_desc = new_desc .. string.sub(current_line, new_line_begin)
			last = next
		until last == nil
		descs[i] = new_desc
	end

end

function WikiDic:RenderCallback()
	if WikiDic.font == nil then
		WikiDic.font = Font()
		WikiDic.font:Load(WikiDic.useDefaultFont and "font/terminus.fnt" or "wd_font/dx_wdic.fnt")
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
			local sprite = WikiDic.targetEntity:GetSprite()
			-- I have to create a new vector, instead of use the old one.
			local oldScale = Vector(sprite.Scale.X,sprite.Scale.Y)
			sprite.Scale = WikiDic.iconScale
			sprite:RenderLayer(1,next_line + icon_offset,WikiDic.nullVector,WikiDic.nullVector)
			sprite.Scale = oldScale
		end
		if WikiDic.targetEntity.Variant == 350 then
			local icon_offset = WikiDic.trinketIconOffset
			local sprite = WikiDic.targetEntity:GetSprite()
			local oldScale = Vector(sprite.Scale.X,sprite.Scale.Y)
			sprite.Scale = WikiDic.iconScale
			sprite:RenderLayer(0,next_line + icon_offset,WikiDic.nullVector,WikiDic.nullVector)
			sprite.Scale = oldScale
		end
		repeat
			local next = string.find(desc,"\n",last+1)
			WikiDic.font:DrawStringScaledUTF8(string.sub(desc,last+1,(string.sub(desc,next or #desc,next) == '\n') and (next or #desc) - 1 or next),next_line.X,next_line.Y,WikiDic.fontScale,WikiDic.fontScale,KColor(1,1,1,0.6,0,0,0),0,true)
			next_line = next_line + Vector(0,WikiDic.font:GetLineHeight()*WikiDic.fontScale)
			last = next
		until last == nil
		-- WikiDic.font:DrawStringUTF8 (desc,rpos.X,rpos.Y,KColor(1,1,1,1,0,0,0),0,true)
	end
end


WikiDic:AddCallback(ModCallbacks.MC_POST_PLAYER_UPDATE,WikiDic.Update)
WikiDic:AddCallback(ModCallbacks.MC_POST_RENDER,WikiDic.RenderCallback)
