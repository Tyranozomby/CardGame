﻿---@type number
max_level = "oui"
---@type number
image_id = 500

---@type string
name = "Nom"
---@type number
pa_cost = 2

---@type Target[]
targets = {
	-- Nom, Type, Automatique ou non,Fonction de filtre des cibles potentielles
	CreateTarget("Une cible carte", TargetTypes.Card, false, card_filter),
	CreateTarget("Un joueur", TargetTypes.Player, true),
}

local function card_filter(a_card)
	-- permet uniquement le ciblage de carte ayant comme nom 'Exemple'
	return a_card.Name == "Exemple"
end

-- fonction qui renvoie un booléen si la carte peut être jouée ou non
function precondition()
	-- la carte peut être jouée sans aucun critère spécifiques
	return true
end

description = "une description de la carte qui peut changer"

function do_effect()
	-- le code de l'effet de la carte
end

function on_level_change(oldLevel, newLevel)
	-- fonction appelée quand la carte change de niveau (OPTIONNEL)
end

function on_card_create()
	-- fonction appelée au lancement de la partie
end