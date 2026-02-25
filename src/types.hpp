/*
Random types
*/

#ifndef _TYPES_HPP_
#define _TYPES_HPP_

#include <SDL3/SDL.h>
#include <stdexcept>

/////////////////
// RING BUFFER //

template <typename T, const int N>
struct Ring_Buffer
{
    T buffer[N];
    int ref [N];

    Ring_Buffer()
    {
        for (int i = 0; i < N; i += 1)
            ref[i] = i;
    }

    T &Push_Front(T value)
    {
        this->buffer[this->ref[0]] = value;
        this->ref[N-1] = this->ref[0];
        for (int i = 0; i < N-1; i++)
            this->ref[i] = this->ref[i+1];

        return this->buffer[this->ref[N-1]];
    }
    T &Push_Back(T value)
    {
        this->buffer[this->ref[N-1]] = value;
        this->ref[0] = this->ref[N-1];
        for (int i = N-1; i > 0; i--)
            this->ref[i] = this->ref[i-1];

        return this->buffer[this->ref[0]];
    }
    T &Get(const int i)
    {
        int idx = (i >= 0) ? i : (N+i);
        if (idx < N && idx >= 0)
            return this->buffer[this->ref[idx]];
        else
            throw std::out_of_range("OUT OF RANGE");
    }
    T &End(void)
    {
        return this->Get(N-1);
    }
    const int Size(void)
    {
        return N;
    }
    T &operator[](const int i)
    {
        return this->Get(i);
    }
};

///////////////////
// vecs and recs //

template <typename T>
struct G2
{
    T x;
    T y;

    T Dot(G2<T> v) const { return this->x * v.x + this->y * v.y; }
    T Lenght(void) const { return SDL_sqrt((float)this->Dot(*this)); }

    void operator=(T s) { *this = {s, s}; return; }
    T &operator[](int i)
    {

        if (i >= 0 && i < 2)
            return ((T*)this)[i];
        else
            throw std::out_of_range( (i < 0) ? "index too small" : "index too big" );
    }
    G2<T> operator+(G2<T> v) { return {this->x + v.x, this->y + v.y}; }
    G2<T> operator-(G2<T> v) { return {this->x - v.x, this->y - v.y}; }
    G2<T> operator*(G2<T> v) { return {this->x * v.x, this->y * v.y}; }
    G2<T> operator/(G2<T> v) { return {this->x / v.x, this->y / v.y}; }

    G2<T> operator+(T v) { return {this->x + v, this->y + v}; }
    G2<T> operator-(T v) { return {this->x - v, this->y - v}; }
    G2<T> operator*(T v) { return {this->x * v, this->y * v}; }
    G2<T> operator/(T v) { return {this->x / v, this->y / v}; }
};

struct V2 : G2<float>
{
    void operator=(G2<float> v) { *this = V2{v.x, v.y}; return; }
    void operator=(SDL_FPoint v) { *this = V2{v.x, v.y}; return; }
    void operator=(const SDL_FPoint v) const { *this = V2{v.x, v.y}; return; }
    operator SDL_FPoint() { return *((SDL_FPoint*)this); }
    operator const SDL_FPoint() const { return *((SDL_FPoint*)this); }
};
struct Rect //: SDL_FRect
{
    float x;
    float y;
    float w;
    float h;

    bool Collision(G2<float> point) const
    {
        return point.x >= this->x
            && point.y >= this->y
            && point.x <= this->x + this->w
            && point.y <= this->y + this->h;
    }
    bool Collision(Rect other) const
    {
        return this->Collision(other.pos())
            && this->Collision(other.pos() + other.size()*V2{0,1})
            && this->Collision(other.pos() + other.size()*V2{1,0})
            && this->Collision(other.pos() + other.size()*V2{1,1});
    }
    Rect Intersection( Rect other ) const
    {
        Rect ret;
        ret.x = SDL_max(this->x, other.x);
        ret.y = SDL_max(this->y, other.y);
        ret.w = SDL_min(this->x+this->w, other.x+other.w) - ret.x;
        ret.h = SDL_min(this->y+this->h, other.y+other.h) - ret.y;
        return ret;
    }

    V2 pos() const { return {this->x, this->y}; }
    V2 size() const { return {this->w, this->h}; }

    SDL_Rect IRect_Round() const {
        return SDL_Rect{
            (int) SDL_roundf(this->x),
            (int) SDL_roundf(this->y),
            (int) SDL_roundf(this->w),
            (int) SDL_roundf(this->h),
        };
    }
    SDL_Rect IRect() const {
        return SDL_Rect{
            (int)this->x,
            (int)this->y,
            (int)this->w,
            (int)this->h,
        };
    }
    SDL_FRect sdl() const {
        return *((SDL_FRect*)this);
    }

    float &operator[](int i) const {
        if (i >= 0 && i < 4)
            return ((float*)this)[i];
        else
            throw std::out_of_range( (i < 0) ? "index too small" : "index too big" );
    }
    void operator=(SDL_FRect v) {
        this->x = v.x;
        this->y = v.y;
        this->w = v.w;
        this->h = v.h;
        return;
    }
    operator SDL_FRect() const {
        return this->sdl();
    }
    operator SDL_Rect() const {
        return this->IRect();
    }
};

#endif //ifndef _TYPES_HPP_
