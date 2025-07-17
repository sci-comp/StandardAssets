extends Node

var pcam : PhantomCamera3D

func _ready():
	pcam = get_parent()
	DialogueManager.dialogue_ended.connect(on_dialogue_ended)

func _exit_tree():
	if DialogueManager.dialogue_ended.is_connected(on_dialogue_ended):
		DialogueManager.dialogue_ended.disconnect(on_dialogue_ended)

func on_dialogue_ended(resource : Resource):
	pcam.priority = 0
