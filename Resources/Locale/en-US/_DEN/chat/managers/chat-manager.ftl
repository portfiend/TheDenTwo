# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
chat-manager-entity-subtle-wrap-message = [italic]{ PROPER($entity) ->
    *[false] The {$entityName} {$message}[/italic]
    [true] {CAPITALIZE($entityName)} {$message}[/italic]
}

chat-manager-entity-subtle-ooc-wrap-message = [italic](OOC) {$entityName} {$message}[/italic]

# speech verbs

chat-speech-verb-name-tajaran = Tajaran
chat-speech-verb-tajaran-1 = meows
chat-speech-verb-tajaran-2 = mews
chat-speech-verb-tajaran-3 = purrs
