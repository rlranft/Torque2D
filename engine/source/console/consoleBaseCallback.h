//-----------------------------------------------------------------------------
// Copyright (c) 2013 GarageGames, LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

#ifndef _CONSOLEBASECALLBACK_H_
#define _CONSOLEBASECALLBACK_H_

#ifndef _TORQUE_TYPES_H_
#include "platform/types.h"
#endif

#include "console.h"
#include "console/consoleObject.h"

class SimObject;

/// defines a callback category.  there should be one subclass per callback per object.
/// for instance, ScriptObject would have one for onAdd and one for onRemove.
class ConsoleBaseCallback
{
};

// base class for hoding "marshalled" data for  callback.
// for instance, a collision callback may have a subclas of this base
// which holds the object collided with, contact points, etc.
class ConsoleBaseCallbackData
{
public:
	virtual ~ConsoleBaseCallbackData() {}
};

/// callback coders should re-use NoCallbackData to mean
/// "no extra data for this callback"
class VoidCallbackData : public ConsoleBaseCallbackData
{
};

/// callback coders can re-use this single NoCallbackData instance to save space/time.
extern VoidCallbackData voidCallbackData;

class ConsoleObject;

class AbstractConsoleCallbackConstructor
{
protected:

   static AbstractConsoleCallbackConstructor* first;
   AbstractConsoleCallbackConstructor* next;

   StringTableEntry mCallbackName;
   AbstractClassRep::ReturnType mReturnType;
   AbstractClassRep::CallbackFunctionByReturnType mCallbackFunction;

public:

   /// The console system defines all the callbacks it wants.   This happens during the init phase of the
   /// app (globally), and using this AbstractConsoleCallbackConstructor.  What is really happening is that
   /// all the data needed to "connect" this callback with the engine are saved in these "command" objects.
   /// One the engine classes have been inited and have had a chance to declare callbacks ("onAdd" for instance)
   /// Con::init will call this classes (static) connectAllCallbacks to actually connect callbacks with the data
   /// stored here.
   /// Here is where a run-time (init time) error will occur if we try to connect to a callback that was never declared.
   static void connectAllCallbacks();

   AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::StringConsoleCallbackFunc sfunc);
   AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::IntConsoleCallbackFunc    ifunc);
   AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::FloatConsoleCallbackFunc  ffunc);
   AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::VoidConsoleCallbackFunc   vfunc);
   AbstractConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::BoolConsoleCallbackFunc   bfunc);

   /// each subclass represents a single callback to be connected to the engine, and then overrides connectCallback
   /// to run the code for the connection process.  This is the "command" action of this command class.
   virtual bool connectCallback() = 0;
};

/// Simple callbacks that we want to connect can be an instantiation of this templated command class.
template <class T>
class ConsoleCallbackConstructor : public AbstractConsoleCallbackConstructor
{
public:

   /// @name ConsoleCallbackConstructor
   ///
   /// The ConsoleCallback macro wrap the declaration of a ConsoleCallbackConstructor.
   /// The constructor holds data about a class.
   ///
   /// Because it is defined as a global, the constructor for the ConsoleConstructor is called
   /// before execution of main() is started. The constructor is called once for each global
   /// ConsoleCallbackConstructor variable.  No order is guaranteed.
   ///
   /// In Con::init(), AbstractConsoleCallbackConstructor::setup() is called to process this prepared list. Each
   /// item in the list is iterated over and connectCallback is invoked.

   ConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::VoidConsoleCallbackFunc func)
	: AbstractConsoleCallbackConstructor(callbackName, func)
   {}

   ConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::FloatConsoleCallbackFunc func)
	: AbstractConsoleCallbackConstructor(callbackName, func)
   {}

   ConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::IntConsoleCallbackFunc func)
	: AbstractConsoleCallbackConstructor(callbackName, func)
   {}

   ConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::BoolConsoleCallbackFunc func)
	: AbstractConsoleCallbackConstructor(callbackName, func)
   {}

   ConsoleCallbackConstructor(const char *callbackName, AbstractClassRep::StringConsoleCallbackFunc func)
	: AbstractConsoleCallbackConstructor(callbackName, func)
   {}


	bool connectCallback() {
		AbstractClassRep* classRep = T::getStaticClassRep();

		const AbstractClassRep::DeclaredCallback* thisDeclaredCallback = NULL;
		for (S32 i = 0; i < classRep->mDeclaredCallbacks.size(); i++) {
			if (classRep->mDeclaredCallbacks[i].mName == mCallbackName) {
				AssertFatal(classRep->mDeclaredCallbacks[i].mReturnType == mReturnType,
					avar("Tried to connect a ConsoleCallback to '%s:%s' but had the wrong return type.",
					classRep->getClassName(), mCallbackName))

				thisDeclaredCallback = &classRep->mDeclaredCallbacks[i];
				break;
			}
		}

		AssertFatal(thisDeclaredCallback,
			avar("Tried to connect a ConsoleCallback to '%s:%s' which was not found.",
			classRep->getClassName(), mCallbackName))

		AbstractClassRep::ConnectedCallbackEntry cbe;
		cbe.mDeclared = thisDeclaredCallback;
		switch(thisDeclaredCallback->mReturnType) {
		case AbstractClassRep::IntReturn :
			cbe.mFunc.intConsoleCallbackFunc = mCallbackFunction.intConsoleCallbackFunc;
			break;
		case AbstractClassRep::FloatReturn :
			cbe.mFunc.floatConsoleCallbackFunc = mCallbackFunction.floatConsoleCallbackFunc;
			break;
		case AbstractClassRep::StringReturn :
			cbe.mFunc.stringConsoleCallbackFunc = mCallbackFunction.stringConsoleCallbackFunc;
			break;
		case AbstractClassRep::BoolReturn :
			cbe.mFunc.boolConsoleCallbackFunc = mCallbackFunction.boolConsoleCallbackFunc;
			break;
		case AbstractClassRep::VoidReturn :
			cbe.mFunc.voidConsoleCallbackFunc = mCallbackFunction.voidConsoleCallbackFunc;
			break;
		default :
			return false;
		}
		classRep->mConnectedCallbacks.push_back(cbe);
		return true;
	}
};

// Console callback macro
#  define ConsoleCallback(className, methodName, returnType, callbackDataClass)																						\
	static inline returnType cb##className##methodName(className* object, const callbackDataClass* data);														\
	static returnType cb##className##methodName##caster(SimObject* object, const char* _callbackName, const ConsoleBaseCallbackData* data) {					\
		AssertFatal( dynamic_cast<const callbackDataClass*>(data), #callbackDataClass " passed to " #methodName " is not a ConsoleBaseCallbackData!" );	\
		AssertFatal( dynamic_cast<className*>(object), "SimObject passed to " #methodName " is not a " #className "!" );								\
		conmethod_return_##returnType ) cb##className##methodName(dynamic_cast<className*>(object), dynamic_cast<const callbackDataClass*>(data));		\
	}																																					\
	static ConsoleCallbackConstructor<className> className##methodName##cb(#methodName, cb##className##methodName##caster);								\
	static inline returnType cb##className##methodName(className* object, const callbackDataClass* data)

#endif // _CONSOLEBASECALLBACK_H_
