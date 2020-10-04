#include "hsv2rgb.h"
#include "glSetup.h"

#include <vector>
#include <math.h>
#include <iostream>
using namespace std;

#include <Eigen/Dense>
using namespace Eigen;

void render(GLFWwindow* window);
void keyboard(GLFWwindow* window, int key, int code, int action, int mods);
void mouse(GLFWwindow* window, int button, int action, int mods);
void drag(GLFWwindow* window, double x, double y);
void drawBSpline();
Vector3f BsplinePoint(Vector3f b[4], float t1);
void drawPoint();
void init();

GLfloat bgColor[4] = { 1, 1, 1, 1 };

Vector3f ee_goal;

int condition = 0;
int selectedPoint;
bool isDragged = false;

int main(int argc, char* argv[])
{
	perspectiveView = false;

	GLFWwindow* window = initializeOpenGL(argc, argv, bgColor);
	if (window == NULL) return -1;

	glfwSetKeyCallback(window, keyboard);
	glfwSetCursorPosCallback(window, drag);
	glfwSetMouseButtonCallback(window, mouse);

	glDisable(GL_DEPTH_TEST);

	reshape(window, windowW, windowH);

	while (!glfwWindowShouldClose(window))
	{
		render(window);
		glfwSwapBuffers(window);
		glfwPollEvents();
	}
	glfwDestroyWindow(window);
	glfwTerminate();
}

vector<Vector3f> p;
void init()
{
	int re = 2;
	for (int i = 0; i < re; i++)
	{
		p.insert(p.begin(), p[0]);
		p.push_back(p[p.size() - 1]);
	}
}
void drawBSpline()
{
	int N_POINTS_PER_SEGMENTS = 40;
	glLineWidth(1.5*dpiScaling);

	init();
	float hsv[3] = { 0, 1, 1 };
	float rgb[3];

	Vector3f b[4];
	for (int i = 0; i < p.size() - 3; i++)
	{
		hsv[0] = 360.0 * i / (p.size() - 3);
		HSV2RGB(hsv, rgb);
		glColor3f(rgb[0], rgb[1], rgb[2]);

		for (int j = 0; j < 4; j++)
			b[j] = p[i + j];

		glBegin(GL_LINE_STRIP);
		for (int j = 0; j < N_POINTS_PER_SEGMENTS; j++)
		{
			float t = (float)j / (N_POINTS_PER_SEGMENTS - 1);

			Vector3f pt = BsplinePoint(b, t);
			glVertex3fv(pt.data());
		}
		glEnd();
	}

	p.erase(p.begin());
	p.erase(p.begin());
	p.pop_back();
	p.pop_back();
}

void drawPoint()
{
	glPointSize(10);
	glColor3f(1, 0, 0);
	glBegin(GL_POINTS);
	for (int i = 0; i < p.size(); i++)
	{
		glVertex3f(p[i][0], p[i][1], p[i][2]);
	}
	glEnd();
}

Vector3f BsplinePoint(Vector3f b[4], float t1)
{
	float t2 = t1 * t1;
	float t3 = t2 * t1;

	float B0 = 1 - 3 * t1 + 3 * t2 - t3;
	float B1 = 4 - 6 * t2 + 3 * t3;
	float B2 = 1 + 3 * t1 + 3 * t2 - 3 * t3;
	float B3 = t3;

	return (b[0] * B0 + b[1] * B1 + b[2] * B2 + b[3] * B3) / 6;
}

void render(GLFWwindow* window)
{
	glClearColor(bgColor[0], bgColor[1], bgColor[2], bgColor[3]);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	drawPoint();
	if (p.size() > 3)
		drawBSpline();
}

void keyboard(GLFWwindow* window, int key, int scancode, int action, int mods)
{
	if (action == GLFW_PRESS || action == GLFW_REPEAT)
	{
		switch (key)
		{
			//Quit
		case GLFW_KEY_Q:
		case GLFW_KEY_ESCAPE: glfwSetWindowShouldClose(window, GL_TRUE); break;
		case GLFW_KEY_A: condition = 1; cout << "Add Point" << endl; break;
		case GLFW_KEY_R: condition = 2; cout << "select/remove" << endl; break;
		case GLFW_KEY_D: condition = 3; cout << "select/drag" << endl; break;
		case GLFW_KEY_I: condition = 4; cout << "select/insert" << endl; break;
		}
	}
}

int selectPoint(Vector3f vec)
{
	int index;
	float min = 100;

	//find nearest point
	for (int i = 0; i < p.size(); i++)
	{
		float x, y;
		x = (p[i][0] - vec[0]) * (p[i][0] - vec[0]);
		y = (p[i][1] - vec[1]) * (p[i][1] - vec[1]);
		if (min > x + y)
		{
			min = x + y;
			index = i;
		}
	}
	return index;
}

void drag(GLFWwindow* window, double x, double y)
{
	if (isDragged)
	{
		float aspect = (float)screenW / screenH;
		x = 2.0 * (x / screenW - 0.5) * aspect;
		y = -2.0 * (y / screenH - 0.5);
		ee_goal[0] += x - ee_goal[0];
		ee_goal[1] += y - ee_goal[1];
		Vector3f newPosition = Vector3f(ee_goal[0], ee_goal[1], .0);
		p[selectedPoint] = newPosition;
	}
}

void selectCurve()
{
	Vector3f index;
	int idxPoint;
	float min = 100;
	float length;

	//find Point
	for (int i = 1; i < p.size(); i++)
	{
		Vector3f segment = p[i] - p[i - 1];
		float ptLength = sqrt(segment[0] * segment[0] + segment[1] * segment[1]);

		Vector3f cursor = ee_goal - p[i - 1];
		float cursorLength = sqrt(cursor[0] * cursor[0] + cursor[1] * cursor[1]);

		Vector3f cursor2 = ee_goal - p[i];
		float cursor2Length = sqrt(cursor2[0] * cursor2[0] + cursor2[1] * cursor2[1]);

		float dot = cursor.dot(segment);
		//proj vector
		Vector3f proj;
		proj[0] = p[i - 1][0] + dot / (ptLength * ptLength) * segment[0];
		proj[1] = p[i - 1][1] + dot / (ptLength * ptLength) * segment[1];
		proj[2] = 0;

		//range check
		float cos = 0;
		if (p[i-1][0] < proj[0] && proj[0] < p[i][0])
		{
			if (p[i - 1][1] < proj[1] && proj[1] < p[i][1])
				cos = 1;
			if (p[i - 1][1] > proj[1] && proj[1] > p[i][1])
				cos = 1;
		}
		else if (p[i - 1][0] > proj[0] && proj[0] > p[i][0])
		{
			if (p[i - 1][1] > proj[1] && proj[1] > p[i][1])
				cos = 1;
			if (p[i - 1][1] < proj[1] && proj[1] < p[i][1])
				cos = 1;
		}

		Vector3f point;
		//point is inline
		if (cos == 1)
		{
			float projLength = fabs(dot / ptLength);
			// length
			length = sqrt(cursorLength * cursorLength - projLength * projLength);
			point = proj;
		}
		else
		{
			//select close Point
			if (cursor2Length <= cursorLength)
			{
				length = cursor2Length;
				point[0] = p[i][0];
				point[1] = p[i][1];
			}
			else
			{
				length = cursorLength;
				point[0] = p[i - 1][0];
				point[1] = p[i - 1][1];
			}
		}

		if (min > length)
		{
			min = length;

			index = Vector3f(point[0], point[1], 0);
			idxPoint = i;
		}
	}

	//insert Point
	p.insert(p.begin() + idxPoint, index);
}

void mouse(GLFWwindow* window, int button, int action, int mods)
{
	if (button == GLFW_MOUSE_BUTTON_LEFT)
	{
		if (action == GLFW_PRESS)
		{
			//find cursor
			double xpos, ypos;
			glfwGetCursorPos(window, &xpos, &ypos);
			ee_goal = Vector3f(xpos, ypos, 0);

			float aspect = (float)screenW / screenH;
			ee_goal[0] = 2.0 * (ee_goal[0] / screenW - 0.5) * aspect;
			ee_goal[1] = -2.0 * (ee_goal[1] / screenH - 0.5);

			switch (condition)
			{
			//a key
			case 1:
			{
				p.push_back(ee_goal);
				break;
			}
			//r key
			case 2:
			{
				int idx = selectPoint(ee_goal);
				p.erase(p.begin() + idx);
				break;
			}
			//d key
			case 3:
			{
				isDragged = true;
				selectedPoint = selectPoint(ee_goal);
				break;
			}
			//i key
			case 4:
			{
				selectCurve();
				break;
			}

			}
		}
		if (action == GLFW_RELEASE)
			isDragged = false;
	}
}
