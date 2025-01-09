extends Node
class_name FoliageShaderController

var _grass_node: MultiMeshInstance3D
var _sun: DirectionalLight3D
var _moon: DirectionalLight3D
var _update_interval: float
var _timer: float = 0.0

@export var update_in_editor: bool = false:
	set(value):
		update_in_editor = value
		if Engine.is_editor_hint() and update_in_editor:
			set_process(true)  # Enable _process in the editor if update_in_editor is true
		else:
			set_process(false)

func initialize(sun: DirectionalLight3D, moon: DirectionalLight3D, update_interval: float):
	_sun = sun
	_moon = moon
	_update_interval = update_interval
	if _sun == null or _moon == null:
		push_error("Error: DirectionalLight3D not set in GrassShaderController.")
	_grass_node = get_parent()
	if not _grass_node:
		push_error("Error: Could not find parent MultiMeshInstance3D node in foliage_shader_controller.gd")

	_update_shader_parameters()

func _process(delta):
	if not Engine.is_editor_hint() or update_in_editor: # Run in game or if update_in_editor is true
		_timer += delta
		if _timer >= _update_interval:
			_timer = 0.0
			_update_shader_parameters()

func _update_shader_parameters():
	var material = _grass_node.multimesh.mesh.surface_get_material(0)
	if material:
		if _sun != null:
			# Get the light direction (pointing towards the light)
			var sun_dir = -_sun.global_transform.basis.z.normalized()
			material.set_shader_parameter("light_direction", sun_dir)
			material.set_shader_parameter("light_color", _sun.light_color)
		elif _moon != null:
			var moon_dir = -_moon.global_transform.basis.z.normalized()
			material.set_shader_parameter("light_direction", moon_dir)
			material.set_shader_parameter("light_color", _moon.light_color)

func _ready():
	if Engine.is_editor_hint() and update_in_editor:
		set_process(true)  # Enable _process in the editor if update_in_editor is true
	else:
		set_process(false)
