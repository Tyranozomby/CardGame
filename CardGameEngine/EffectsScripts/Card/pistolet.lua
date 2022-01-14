﻿---@module pistolet
max_level = 2
image_id = 569

name = "Pistolet"
pa_cost = 2

description = "lvl 1 -> Défausse la carte au plus faible cout de la main de l'adversaire, lvl2 -> défausse celle au plus haut cout"

function cardFilter()
    local OtherHand = EffectOwner.OtherPlayer.Hand
    if (OtherHand.Count == 0) then
        return nil
    elseif (This.CurrentLevel.Value == 1) then
        local minCardCost = OtherHand[0].Cost.Value
        local minCard = OtherHand[0]
        for card in OtherHand do
            if (card.Cost.Value < minCardCost) then
                minCardCost = card.Cost.Value
                minCard = card
            end
        end
        return minCard
    else
        local maxCardCost = OtherHand[1].Cost.Value
        local maxCard = OtherHand[1]
        for card in OtherHand do
            if (card.Cost.Value > maxCardCost) then
                maxCardCost = card.Cost.Value
                maxCard = card
            end
        end
        return maxCard.test
    end
end

targets = {
    CreateTarget("défausse la carte la plus faible en cout de l'adversaire", TargetTypes.Card, true, cardFilter),
}

function precondition()
    return EffectOwner.OtherPlayer.Hand.Count > 0
end

function do_effect()
    local carte = AskForTarget(1)                                                     --carte
    EffectOwner.OtherPlayer.Hand.MoveTo(EffectOwner.OtherPlayer.Discard, carte, 0)    --prends la carte de l'adversaire depuis la main et la met dans sa defausse
end