extends RefCounted
class_name GdUnit4MonoAPILoader

static func instance() :
	return null#GdUnitSingleton.get_or_create_singleton("GdUnit3MonoAPI", "res://addons/gdUnit4/src/mono/GdUnit3MonoAPI.cs")


static func create_test_suite(source_path :String, line_number :int, test_suite_path :String) -> Dictionary:
	return instance().CreateTestSuite(source_path, line_number, test_suite_path) as Dictionary


static func is_test_suite(resource_path :String) -> bool:
	return instance().IsTestSuite(resource_path)


static func parse_test_suite(source_path :String) -> Node:
	return instance().ParseTestSuite(source_path)


static func create_executor(listener :Node) -> Node:
	return instance().Executor(listener)


