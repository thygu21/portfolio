#ifndef __GL_SETUP_H_
#define __GL_SETUP_H_

#include <GL/glew.h>
#include <GLFW/glfw3.h>

extern bool fullScreen;
extern bool noMenuBar;

extern int vsync;
extern bool perspectiveView;
extern float fovy;

extern float screenScale;
extern int screenW, screenH;
extern int windowW, windowH;
extern float aspect;
extern float dpiScaling;

GLFWwindow* initializeOpenGL(int argc, char* argv[], GLfloat bg[4], bool modern = false);
void reshape(GLFWwindow* window, int w, int h);

void drawAxes(float l, float w);

#endif 