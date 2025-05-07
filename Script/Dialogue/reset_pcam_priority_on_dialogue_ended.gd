extends Node

var pcam : PhantomCamera3D

func _ready():
	pcam = get_parent()
	DialogueManager.dialogue_ended.connect(on_dialogue_ended)

func on_dialogue_ended(resource : Resource):
	pcam.priority = 0
