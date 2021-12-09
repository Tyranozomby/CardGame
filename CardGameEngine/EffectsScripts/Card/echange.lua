﻿max_level = 1   
image_id = 514


name = "Echange"
pa_cost = 2

targets = {
    CreateTarget("La carte que l'on veut echanger", TargetTypes.Card, true, card_filter),
}

function card_filter() -- Verifier quand pas de carte
    local OtherHand = EffectOwner.OtherPlayer.Hand
    math.randomseed(os.time())
    local random = math.random(0,OtherHand.Count()-1)
    return OtherHand[random]
end 

-- fonction qui renvoie un booléen si la carte peut être jouée ou non
function precondition()
    return TargetsExists({1})
end 

function description()
    return "Echange cette carte avec une aléatoire de la main de votre adversaire"
end

function do_effect() 
    local theCard = AskForTarget(1)
    EffectOwner.OtherPlayer.Hand.MoveTo(EffectOwner.Hand,theCard,0)
    EffectOwner.Hand.MoveTo(EffectOwner.OtherPlayer.Hand,ThisCard,0)
     
     
end

