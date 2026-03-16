# https://cmake.org/cmake/help/latest/prop_dir/BUILDSYSTEM_TARGETS.html
if(NOT COMMAND uud_silence_warnings_in_dir)
    function(uud_silence_warnings_in_dir dir)
        get_property(targets DIRECTORY "${dir}" PROPERTY BUILDSYSTEM_TARGETS)
        foreach(target IN LISTS targets)
            get_target_property(type "${target}" TYPE)
            if(type STREQUAL "STATIC_LIBRARY" OR type STREQUAL "SHARED_LIBRARY"
               OR type STREQUAL "MODULE_LIBRARY" OR type STREQUAL "OBJECT_LIBRARY"
               OR type STREQUAL "EXECUTABLE")
                target_compile_options("${target}" PRIVATE -w)
            endif()
        endforeach()

        get_property(subdirs DIRECTORY "${dir}" PROPERTY SUBDIRECTORIES)
        foreach(subdir IN LISTS subdirs)
            uud_silence_warnings_in_dir("${subdir}")
        endforeach()
    endfunction()
endif()

macro(uud_fetch_content_quiet)
    set(_uud_prev_warn_deprecated "${CMAKE_WARN_DEPRECATED}")
    set(_uud_prev_suppress_dev "${CMAKE_SUPPRESS_DEVELOPER_WARNINGS}")
    set(CMAKE_WARN_DEPRECATED OFF)
    set(CMAKE_SUPPRESS_DEVELOPER_WARNINGS 1)

    FetchContent_MakeAvailable(${ARGN})

    set(CMAKE_WARN_DEPRECATED "${_uud_prev_warn_deprecated}")
    set(CMAKE_SUPPRESS_DEVELOPER_WARNINGS "${_uud_prev_suppress_dev}")
endmacro()
