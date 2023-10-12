extends Node

@export var mouse_sensitivity: float = 0.05
@export var min_yaw: float = -89.9
@export var max_yaw: float = 50
@export var min_pitch: float = 0
@export var max_pitch: float = 360

@onready var _camera: Camera3D = %MainCamera3D
@onready var _player_pcam: PhantomCamera3D = %PlayerPhantomCamera3D

func _ready():
	if _player_pcam.get_follow_mode() == _player_pcam.Constants.FollowMode.THIRD_PERSON:
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _unhandled_input(event: InputEvent) -> void:
	if _player_pcam.get_follow_mode() == _player_pcam.Constants.FollowMode.THIRD_PERSON:
		_set_pcam_rotation(_player_pcam, event)

func _set_pcam_rotation(pcam: PhantomCamera3D, event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		var pcam_rotation_degrees: Vector3
		
		# Assigns the current 3D rotation of the SpringArm3D node - so it starts off where it is in the editor 
		pcam_rotation_degrees = pcam.get_third_person_rotation_degrees()
		
		# Change the X rotation
		pcam_rotation_degrees.x -= event.relative.y * mouse_sensitivity
		
		# Clamp the rotation in the X axis so it go over or under the target
		pcam_rotation_degrees.x = clampf(pcam_rotation_degrees.x, min_yaw, max_yaw)

		# Change the Y rotation value
		pcam_rotation_degrees.y -= event.relative.x * mouse_sensitivity
		
		# Sets the rotation to fully loop around its target, but witout going below or exceeding 0 and 360 degrees respectively
		pcam_rotation_degrees.y = wrapf(pcam_rotation_degrees.y, min_pitch, max_pitch)
		
		# Change the SpringArm3D node's rotation and rotate around its target
		pcam.set_third_person_rotation_degrees(pcam_rotation_degrees)


