player-requirement-trait = [color=LightBlue]{$traitName}[/color]
player-requirement-trait-reason = Must{$inverted ->
    [true] not
   *[false] {" "}
} have {$constraint} of these traits: {$traits};
