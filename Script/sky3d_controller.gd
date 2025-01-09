@tool
extends Node
class_name Sky3DController

@onready var sun: DirectionalLight3D = $Sky3D/SunLight
@onready var moon: DirectionalLight3D = $Sky3D/MoonLight
@onready var sky3D: Node = $Sky3D

# TODO: grass should be an array, and it will need to be populated on level loaded
@export var grass: MultiMeshInstance3D

var timer: float = 0.0

@export var update_in_editor: bool = false:
	set(value):
		update_in_editor = value
		set_process(update_in_editor)

func _ready():
	set_process(update_in_editor or not Engine.is_editor_hint())
	print("[EnvironmentController] Ready")

func _process(delta):
	timer += delta
	if timer >= sky3D.get("update_interval"):
		timer = 0.0
		update_shader_parameters()

func update_shader_parameters():
	if grass:
		var material: Material = grass.material_override
		
		if material == null:
			if grass.multimesh and grass.multimesh.mesh:
				material = grass.multimesh.mesh.surface_get_material(0)
		
		if material:
			set_shader_params(material, grass)

func set_shader_params(material: Material, multimesh_node: MultiMeshInstance3D):
	if sun:
		# Get the direction the sun is shining FROM
		var sun_dir = -sun.global_transform.basis.z
		material.set_shader_parameter("light_direction", sun_dir)
		material.set_shader_parameter("light_color", sun.light_color)
	elif moon:
		# Get the direction the moon is shining FROM
		var moon_dir = -moon.global_transform.basis.z
		material.set_shader_parameter("light_direction", moon_dir)
		material.set_shader_parameter("light_color", moon.light_color)
