player-requirement-playtime-time = {$playtime} minutes
player-requirement-playtime-minimum-time = at least {$playtime}
player-requirement-playtime-maximum-time = less than {$playtime}
player-requirement-playtime-minmax-time = between {$minimum} and {$maximum}
player-requirement-playtime-constraint-reason = Must {$inverted ->
    [true] not
   *[false] {""}
} have {$timeConstraint}

player-requirement-department-playtime-reason = {$constraint} in the {$department} department.
