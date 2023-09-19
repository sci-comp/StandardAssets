extends CharacterBody3D

var mouse_sensitivity := 0.001
var twist_input := 0.0
var pitch_input := 0.0
var speed := 12.0
var gravity := Vector3(0, -9.8, 0)

@onready var twist_pivot := $TwistPivot
@onready var pitch_pivot := $TwistPivot/PitchPivot

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _physics_process(delta):
	var input := Vector3.ZERO
	input.x = Input.get_axis("move_left", "move_right")
	input.z = Input.get_axis("move_forward", "move_backward")

	# Apply gravity
	self.velocity += gravity * delta
	
	# Apply input-based movement
	self.velocity.x = input.x * speed
	self.velocity.z = input.z * speed
	
	# Move and slide
	move_and_slide()
	
	# Handle mouse-look
	twist_pivot.rotate_y(twist_input)
	pitch_pivot.rotate_x(pitch_input)
	pitch_pivot.rotation.x = clamp(pitch_pivot.rotation.x, -0.5, 0.5)
	
	# Reset inputs
	twist_input = 0
	pitch_input = 0

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			twist_input = -event.relative.x * mouse_sensitivity
			pitch_input = -event.relative.y * mouse_sensitivity * 0.7
			
	if Input.is_action_just_pressed("move_forward"):
		Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			
	if Input.is_action_just_pressed("ui_cancel"):
		Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
