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
WikiDic.taintIsaacOffset = Vector(0,25)
WikiDic.taintQQQOffset = Vector(0,15)
WikiDic.authRemains = 60*9
WikiDic.lineDistance = 0
WikiDic.authTexts = {}
WikiDic.qrcodeToggle = false
WikiDic.qrcodeToggleCounter = 0
WikiDic.qrcodeToggleCounterInit = 30
WikiDic.qrCodeOffset = Vector(10,0)
WikiDic.questionMarkSprite = nil

WikiDic.huijiTQrCodeInfo = "双击Tab键（地图键）开关二维码显示"
WikiDic.huijiWikiInfo = {
	"按住Tab键（地图键）显示手上持有的卡牌/药丸/符文信息",
	"中文图鉴信息来源：灰机wiki",
	"https://isaac.huijiwiki.com/wiki",
}

WikiDic.fandomWikiInfo = {
	"English information comes from:",
	"The Binding of Isaac:Rebirth wiki",
	"https://bindingofisaacrebirth.fandom.com/"
}

WikiDic.nullVector = Vector(0,0)

-- FAKE_CONFIG_SEG_1 --

--WikiDic.usePlayerPos = true --mouse or player pos
--WikiDic.drawMouse = true
--WikiDic.useDefaultFont = true
--WikiDic.useHalfSizeFont = true
--WikiDic.useBiggerSizeFont = true
--WikiDic.renderQrcode = true --renderQrcode == 1 >> [double click tab] >> WikiDic.qrcodeToggle >> contains_qrcode


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
	WikiDic.taintIsaacOffset = Vector(3,27)
end
if WikiDic.useBiggerSizeFont then
	WikiDic.renderPos = Vector(80,45)
	WikiDic.iconNoShopItemOffset = Vector(-8,40)
	WikiDic.iconOffset = Vector(-15,20)
	WikiDic.trinketIconOffset = Vector(-10,12)
	WikiDic.iconScale = Vector(1.2,1.2)
	WikiDic.fontScale = 1.2
	WikiDic.taintIsaacOffset = Vector(0,25)
	WikiDic.lineDistance = 3
end
if WikiDic.useHuijiWiki then
	if WikiDic.renderQrcode then
		table.insert(WikiDic.authTexts, WikiDic.huijiTQrCodeInfo)
	end

	for _,text in pairs(WikiDic.huijiWikiInfo) do
		table.insert(WikiDic.authTexts,text)
	end
end
if WikiDic.useFandomWiki then
	if WikiDic.useHuijiWiki then
		table.insert(WikiDic.authTexts, "")
	end
	for _,text in pairs(WikiDic.fandomWikiInfo) do
		table.insert(WikiDic.authTexts,text)
	end
end

WikiDic.desc = {
-- FAKE_DESC_CONTENT --
}

WikiDic.trinketDesc = {
-- FAKE_TRINKET_CONTENT --
}

WikiDic.cardDesc = {
-- FAKE_CARD_CONTENT --
}
WikiDic.pillDesc = {
-- FAKE_PILL_CONTENT --
}

function WikiDic:SpriteIsQuestionmark(entitySprite, questionSprite)
	if not entitySprite:IsPlaying("Idle") then
		return false
	end

	questionSprite:SetFrame("Idle",entitySprite:GetFrame())
	-- check some point, if returns false, it is not same with question mark
	for i = -70,0,2 do
		local qcolor = questionSprite:GetTexel(Vector(0,i),WikiDic.nullVector,1,1)
		local ecolor = entitySprite:GetTexel(Vector(0,i),WikiDic.nullVector,1,1)
		if qcolor.Red ~= ecolor.Red or qcolor.Green ~= ecolor.Green or qcolor.Blue ~= ecolor.Blue then
			return false
		end
	end

	--this may be a question mark, however, we will check it again to ensure it
	for j = -3,3,2 do
		for i = -71,0,2 do
			local qcolor = questionSprite:GetTexel(Vector(j,i),WikiDic.nullVector,1,1)
			local ecolor = entitySprite:GetTexel(Vector(j,i),WikiDic.nullVector,1,1)
			if qcolor.Red ~= ecolor.Red or qcolor.Green ~= ecolor.Green or qcolor.Blue ~= ecolor.Blue then
				return false
			end
		end
	end


	return true
end

function WikiDic:IsQuestionMarkTexture(entity)
	-- use data buffer
	local data = entity:GetData()
	local mark = data["WikiDicQuestionMarkStatus"]
	if mark then
		return mark
	end

	local questionSprite = WikiDic.questionMarkSprite
	local entitySprite = entity:GetSprite()

	if not questionSprite then
		-- we dont show anything before the font init
		return true
	end

	mark = WikiDic:SpriteIsQuestionmark(entitySprite,questionSprite)
	data["WikiDicQuestionMarkStatus"] = mark
	return mark
end

function WikiDic:Update()
	WikiDic.targetEntity = nil
	-- if (Game():GetLevel():GetCurses() & LevelCurse.CURSE_OF_BLIND) ~= 0 then
	-- 	return
	-- end

	local tpos = WikiDic.usePlayerPos and Isaac.GetPlayer(0).Position or Input.GetMousePosition(true)
	for _,e in pairs(Isaac.GetRoomEntities()) do
		if e.Type == 5 and (e.Variant == 100 or e.Variant == 350) then
			if e.Variant == 100 and (e.SubType == 0 or (Game():GetLevel():GetCurses() & LevelCurse.CURSE_OF_BLIND) ~= 0 or WikiDic:IsQuestionMarkTexture(e)) then
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

function WikiDic:InitFonts()
	if WikiDic.font == nil then
		WikiDic.font = Font()
		WikiDic.font:Load(WikiDic.useDefaultFont and "font/terminus.fnt" or "wd_res/font/wdic_font.fnt")
		WikiDic:FixReturn(WikiDic.desc)
		WikiDic:FixReturn(WikiDic.trinketDesc)
		WikiDic:FixReturn(WikiDic.cardDesc)
		WikiDic:FixReturn(WikiDic.pillDesc)
		if WikiDic.renderQrcode then
			WikiDic.qrcodeSprite = Sprite()
			WikiDic.qrcodeSprite:Load("wd_res/qrcode/qrcode.anm2",true)
			WikiDic.qrcodeSprite:SetFrame("show",1)
		end

		WikiDic.questionMarkSprite = Sprite()
		WikiDic.questionMarkSprite:Load("gfx/005.100_collectible.anm2",true)
		WikiDic.questionMarkSprite:ReplaceSpritesheet(1,"gfx/items/collectibles/questionmark.png")
		WikiDic.questionMarkSprite:LoadGraphics()
	end
end

function WikiDic:RandStr(eid)
	local rng = RNG()
	rng:SetSeed((eid + Game():GetSeeds():GetStartSeed()) % 0x100000000,4)
	local str = ""
	local strlen = rng:RandomInt(30) + 20
	local lineStart = 0
	local firstLineLen = rng:RandomInt(4) + 3

	local line_length = 0

	while strlen > 0 do
		strlen = strlen - 1
		local word = rng:RandomInt(0x9FA5 - 0x4E00 + 1) + 0x4E00
		str = str .. string.char(0xE0 + (word >> 12), 0x80 + ((word >> 6) & 0x3F),0x80 + (word & 0x3F))
		if firstLineLen == 0 or (WikiDic.font:GetStringWidth(string.sub(str,lineStart)) > WikiDic.lineWrap or (#str - lineStart > 20 and rng:RandomInt(8) == 1)) then
			str = str .. '\n'
			lineStart = #str
		end
		firstLineLen = firstLineLen - 1
	end
	return str
end

function WikiDic:RenderCallback()
	WikiDic:InitFonts()

	local is_Jacob_and_Esau = Isaac.GetPlayer(0).SubType == 19 and Isaac.GetPlayer(1).SubType == 20
	local player_index = is_Jacob_and_Esau and Input.IsActionPressed(ButtonAction.ACTION_DROP,Isaac.GetPlayer(0).ControllerIndex) and 1 or 0
	
	local itemPool = Game():GetItemPool()
	local pill_color = Isaac.GetPlayer(player_index):GetPill(0)
	local card_hold = WikiDic.useHuijiWiki and Input.IsActionPressed(ButtonAction.ACTION_MAP,Isaac.GetPlayer(0).ControllerIndex) and Isaac.GetPlayer(player_index):GetCard(0) or 0
	local pill_hold = WikiDic.useHuijiWiki and Input.IsActionPressed(ButtonAction.ACTION_MAP,Isaac.GetPlayer(0).ControllerIndex) and itemPool:IsPillIdentified(pill_color) and itemPool:GetPillEffect(pill_color) or -1
	if pill_hold == 31 then
		-- ??? pill
		pill_hold = -1
	end
	-- draw auth infos here
	if WikiDic.authRemains > 0 then
		if WikiDic.targetEntity ~= nil or Input.IsActionPressed(ButtonAction.ACTION_MAP,Isaac.GetPlayer(0).ControllerIndex) then
			WikiDic.authRemains = 0
		end

		WikiDic.authRemains = WikiDic.authRemains - 1
		local screen_center = Isaac.WorldToRenderPosition(Vector(320,240))

		local y_pos = screen_center.Y - #WikiDic.authTexts * WikiDic.font:GetLineHeight() / 2
		local alpha = 0.6 --math.cos(WikiDic.authRemains * 0.1) * 0.1 + 0.5
		if WikiDic.authRemains < 120 then
			alpha = alpha * WikiDic.authRemains / 120
		end

		for _,text in pairs(WikiDic.authTexts) do
			WikiDic.font:DrawStringScaledUTF8(text,WikiDic.renderPos.X,y_pos,WikiDic.fontScale,WikiDic.fontScale,KColor(1,1,1,alpha,0,0,0),0,true)
			y_pos = y_pos + WikiDic.font:GetLineHeight() * WikiDic.fontScale + WikiDic.lineDistance
		end
		return
	end

	local contains_qrcode = false
	--toggle qrcode
	local map_btn_pressed = Input.IsActionTriggered(ButtonAction.ACTION_MAP,Isaac.GetPlayer(0).ControllerIndex)
	if WikiDic.qrcodeToggleCounter > 0 then
		WikiDic.qrcodeToggleCounter = WikiDic.qrcodeToggleCounter - 1
		if map_btn_pressed then
			WikiDic.qrcodeToggleCounter = 0
			WikiDic.qrcodeToggle = not WikiDic.qrcodeToggle
		end
	elseif WikiDic.renderQrcode and map_btn_pressed then
		WikiDic.qrcodeToggleCounter = WikiDic.qrcodeToggleCounterInit
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

	if WikiDic.targetEntity ~= nil or card_hold ~= 0 or pill_hold ~= -1 then
		local desc = nil
		local last = 0
		local next_line = WikiDic.renderPos --Vector(rpos.X,rpos.Y)
		-- taint isaac is 21
		if Isaac.GetPlayer(0).SubType == 21 then
			next_line = next_line + WikiDic.taintIsaacOffset
		end
		-- taint ??? is 25
		if Isaac.GetPlayer(0).SubType == 25 then
			next_line = next_line + WikiDic.taintQQQOffset
		end
		if card_hold ~= 0 then
			desc = WikiDic.cardDesc[card_hold] or (tostring(card_hold) .. "号卡牌没有收录") 
		elseif pill_hold ~= -1 then
			desc = WikiDic.pillDesc[pill_hold] or (tostring(pill_hold) .. "药丸效果没有收录")
			if pill_color > 2048 then
				desc = desc .. "\n(效果增强)"
			end
		elseif WikiDic.targetEntity ~= nil then
			if WikiDic.targetEntity.Variant == 100 then
				--setup desc
				local entity_id = WikiDic.targetEntity.SubType
				if (entity_id & 0x80000000) ~= 0 then
					-- TMTRAINER
					-- this is a very simple random string buffer
					if WikiDic.lastRandTarget ~= entity_id or WikiDic.randText == nil then
						WikiDic.randText = WikiDic:RandStr(entity_id)
						WikiDic.lastRandTarget = entity_id
					end
					desc = WikiDic.randText
				else
					desc = WikiDic.desc[entity_id] or (tostring(entity_id) .. "号道具\n\n没有收录")
				end
				--draw icon
				local icon_offset = (not WikiDic.targetEntity:ToPickup():IsShopItem()) and WikiDic.iconNoShopItemOffset or WikiDic.iconOffset
				local sprite = WikiDic.targetEntity:GetSprite()
				-- I have to create a new vector, instead of use the old one.
				local oldScale = Vector(sprite.Scale.X,sprite.Scale.Y)
				sprite.Scale = WikiDic.iconScale
				sprite:RenderLayer(1,next_line + icon_offset,WikiDic.nullVector,WikiDic.nullVector)
				sprite.Scale = oldScale

				--replace qrcode sprite
				if WikiDic.qrcodeToggle then
					contains_qrcode = true
					local target_graph = "wd_res/qrcode/item_" .. tostring(entity_id) .. ".png"
					if WikiDic.qrcodeGraph ~= target_graph then
						WikiDic.qrcodeSprite:ReplaceSpritesheet(0,target_graph)
						WikiDic.qrcodeSprite:LoadGraphics()
						WikiDic.qrcodeGraph = target_graph
					end
 				end
			end
			if WikiDic.targetEntity.Variant == 350 then
				local trinket_id = WikiDic.targetEntity.SubType
				local is_gold = false
				if trinket_id > 32768 then
					is_gold = true
					trinket_id = trinket_id - 32768
				end
				--setup desc
				desc = WikiDic.trinketDesc[trinket_id] or (tostring(trinket_id) .. "号饰品\n\n没有收录")
				if is_gold then
					if WikiDic.useHuijiWiki then
						desc = "(金色的)" .. desc 
					else
						desc = "(Golden) " .. desc
					end
				end
				--draw icon
				local icon_offset = WikiDic.trinketIconOffset
				local sprite = WikiDic.targetEntity:GetSprite()
				local oldScale = Vector(sprite.Scale.X,sprite.Scale.Y)
				sprite.Scale = WikiDic.iconScale
				sprite:RenderLayer(0,next_line + icon_offset,WikiDic.nullVector,WikiDic.nullVector)
				sprite.Scale = oldScale

				--replace qrcode sprite
				if WikiDic.qrcodeToggle then
					contains_qrcode = true
					local target_graph = "wd_res/qrcode/trinket_" .. tostring(trinket_id) .. ".png"
					if WikiDic.qrcodeGraph ~= target_graph then
						WikiDic.qrcodeSprite:ReplaceSpritesheet(0,target_graph)
						WikiDic.qrcodeSprite:LoadGraphics()
						WikiDic.qrcodeGraph = target_graph
					end
				end
			end
		end

		--draw text
		repeat
			local next = string.find(desc,"\n",last+1)
			WikiDic.font:DrawStringScaledUTF8(string.sub(desc,last+1,(string.sub(desc,next or #desc,next) == '\n') and (next or #desc) - 1 or next),next_line.X,next_line.Y,WikiDic.fontScale,WikiDic.fontScale,KColor(1,1,1,0.6,0,0,0),0,true)
			next_line = next_line + Vector(0,WikiDic.font:GetLineHeight()*WikiDic.fontScale + WikiDic.lineDistance)
			last = next
		until last == nil
		--draw qrcode
		if contains_qrcode then
			WikiDic.qrcodeSprite:RenderLayer(0,next_line + WikiDic.qrCodeOffset,WikiDic.nullVector,WikiDic.nullVector)
		end
		-- WikiDic.font:DrawStringUTF8 (desc,rpos.X,rpos.Y,KColor(1,1,1,1,0,0,0),0,true)
	end
end

WikiDic:AddCallback(ModCallbacks.MC_POST_PLAYER_UPDATE,WikiDic.Update)
WikiDic:AddCallback(ModCallbacks.MC_POST_RENDER,WikiDic.RenderCallback)
